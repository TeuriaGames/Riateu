using GpuBuffer = MoonWorks.Graphics.Buffer;
using System.Collections.Generic;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Riateu.Graphics;


/// <summary>
/// A batch system used to batch all of the vertices in one draw calls while it can.
/// This also utilizes a texture swapping which would add additional sub batches to be
/// able to draw multiple textures.
/// </summary>
public class Batch : System.IDisposable
{
    private struct BatchQueue 
    {
        public uint Count;
        public TextureSamplerBinding Binding;
        public GraphicsPipeline Pipeline;
    }
    private const uint MaxTextures = 4096;
    private const uint InitialMaxQueues = 4;
    private GraphicsDevice device;
    private unsafe ComputeData* computes;

    private Stack<Matrix4x4> Matrices;
    private bool rendered;

    private GpuBuffer vertexBuffer;
    private GpuBuffer indexBuffer;
    private GpuBuffer computeBuffer;
    private TransferBuffer transferComputeBuffer;
    private BatchQueue[] queues = new BatchQueue[InitialMaxQueues];
    private uint onQueue = uint.MaxValue;

#if DEBUG
    private bool DEBUG_begin;
#endif


    public uint VertexIndex;
    public uint CurrentMaxTexture = MaxTextures;

    /// <summary>
    /// A current matrix projection to be used for rendering.
    /// </summary>
    public Matrix4x4 Matrix;
    /// <summary>
    /// A check if the <see cref="Riateu.Graphics.Batch"/> is already been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// An initialization for the batch system.
    /// </summary>
    /// <param name="device">An application graphics device</param>
    /// <param name="width">A width of a orthographic matrix</param>
    /// <param name="height">A height of a orthographic matrix</param>
    public Batch(GraphicsDevice device, int width, int height)
    {
        Matrices = new();
        this.device = device;

        vertexBuffer = GpuBuffer.Create<PositionTextureColorVertex>(device, 
            BufferUsageFlags.Vertex 
            | BufferUsageFlags.ComputeStorageRead
            | BufferUsageFlags.ComputeStorageWrite, MaxTextures * 4);
        indexBuffer = GenerateIndexArray(device, MaxTextures * 6);

        transferComputeBuffer = TransferBuffer.Create<ComputeData>(device, TransferBufferUsage.Upload, MaxTextures);
        computeBuffer = GpuBuffer.Create<ComputeData>(device, BufferUsageFlags.ComputeStorageRead, MaxTextures);

        var view = Matrix4x4.CreateTranslation(0, 0, 0);
        var projection = Matrix4x4.CreateOrthographicOffCenter(0, width, 0, height, -1, 1);
        Matrix = view * projection;
    }

    private static unsafe GpuBuffer GenerateIndexArray(GraphicsDevice device, uint maxIndices)
    {
        using TransferBuffer transferBuffer = TransferBuffer.Create<uint>(device, TransferBufferUsage.Upload, maxIndices);
        GpuBuffer indexBuffer = GpuBuffer.Create<uint>(device, BufferUsageFlags.Index, maxIndices);

        transferBuffer.Map(false, out byte* mapPtr);
        uint* indexPtr = (uint*)mapPtr;

        for (uint i = 0, j = 0; i < maxIndices; i += 6, j += 4)
        {
            indexPtr[i] = j;
            indexPtr[i + 1] = j + 1;
            indexPtr[i + 2] = j + 2;
            indexPtr[i + 3] = j + 2;
            indexPtr[i + 4] = j + 1;
            indexPtr[i + 5] = j + 3;
        }
        transferBuffer.Unmap();

        CommandBuffer commandBuffer = device.AcquireCommandBuffer();
        CopyPass copyPass = commandBuffer.BeginCopyPass();
        copyPass.UploadToBuffer(transferBuffer, indexBuffer, false);
        commandBuffer.EndCopyPass(copyPass);
        device.Submit(commandBuffer);

        return indexBuffer;
    }

    /// <inheritdoc/>
    public void Begin(Texture texture, Sampler sampler)
    {
#if DEBUG
        AssertBegin();
        DEBUG_begin = true;
#endif
        if (rendered)
        {
            VertexIndex = 0;
            onQueue = uint.MaxValue;
            rendered = false;
        }

        if (queues.Length == onQueue) 
        {
            Array.Resize(ref queues, queues.Length + 4);
        }

        unchecked { onQueue++; }
        queues[onQueue] = new BatchQueue 
        {
            Binding = new TextureSamplerBinding(texture, sampler),
            Pipeline = GameContext.DefaultPipeline,
            Count = 0
        };

        unsafe {
            transferComputeBuffer.Map(true, out byte* data);
            computes = (ComputeData*)data;
        }
    }

    /// <inheritdoc/>
    public void End(CommandBuffer cmdBuf)
    {
#if DEBUG
        AssertDoesBegin();
        DEBUG_begin = false;
#endif
        transferComputeBuffer.Unmap();
        if (VertexIndex == 0)
        {
            return;
        }
        CopyPass copyPass = cmdBuf.BeginCopyPass();
        copyPass.UploadToBuffer(transferComputeBuffer, computeBuffer, true);
        cmdBuf.EndCopyPass(copyPass);

        ComputePass computePass = cmdBuf.BeginComputePass(new StorageBufferReadWriteBinding 
        {
            Buffer = vertexBuffer,
            Cycle = true
        });
        computePass.BindComputePipeline(GameContext.SpriteBatchPipeline);
        computePass.BindStorageBuffer(computeBuffer);
        computePass.Dispatch(CurrentMaxTexture / 64, 1, 1);

        cmdBuf.EndComputePass(computePass);
        queues[onQueue].Count = VertexIndex;

    }

    /// <inheritdoc/>
    public void PushMatrix(in Matrix4x4 matrix)
    {
        Matrices.Push(Matrix);
        Matrix = matrix;
    }

    /// <inheritdoc/>
    public void PushMatrix(in Camera camera)
    {
        PushMatrix(camera.Transform);
    }

    /// <inheritdoc/>
    public void PopMatrix()
    {
        if (Matrices.Count == 0)
        {
            Logger.LogError("Use of PopMatrix while there is no matrix had pushed yet");
            return;
        }
        Matrix = Matrices.Pop();
    }

    /// <inheritdoc/>
    public void Render(RenderPass renderPass)
    {
        Render(renderPass, Matrix);
    }

    /// <inheritdoc/>
    public void Render<T>(RenderPass renderPass, in T fragmentUniform)
    where T : unmanaged
    {
        Render(renderPass, Matrix);
    }

    /// <inheritdoc/>
    public void Render<T>(RenderPass renderPass, Matrix4x4 viewProjection, in T fragmentUniform)
    where T : unmanaged
    {
#if DEBUG
        AssertRender();
        DEBUG_begin = false;
#endif
        if (VertexIndex == 0)
        {
            return;
        }

        ref var start = ref MemoryMarshal.GetArrayDataReference(queues);
        ref var end = ref Unsafe.Add(ref start, onQueue + 1);

        uint offset = 0;

        while (Unsafe.IsAddressLessThan(ref start, ref end)) 
        {
            renderPass.BindGraphicsPipeline(start.Pipeline);
            renderPass.PushVertexUniformData(viewProjection);
            renderPass.BindVertexBuffer(vertexBuffer);
            renderPass.BindIndexBuffer(indexBuffer, IndexElementSize.ThirtyTwo);
            renderPass.BindFragmentSampler(start.Binding);
            renderPass.PushFragmentUniformData(fragmentUniform);
            renderPass.DrawIndexedPrimitives(offset * 4u, 0u, (start.Count - offset) * 2u, 1);

            offset += start.Count;
            start = ref Unsafe.Add(ref start, 1);
        }

        rendered = true;
        VertexIndex = 0;
    }

    /// <inheritdoc/>
    public void Render(RenderPass renderPass, Matrix4x4 viewProjection)
    {
#if DEBUG
        AssertRender();
        DEBUG_begin = false;
#endif
        if (VertexIndex == 0)
        {
            return;
        }

        ref var start = ref MemoryMarshal.GetArrayDataReference(queues);
        ref var end = ref Unsafe.Add(ref start, onQueue + 1);

        uint offset = 0;

        while (Unsafe.IsAddressLessThan(ref start, ref end)) 
        {
            renderPass.BindGraphicsPipeline(start.Pipeline);
            renderPass.PushVertexUniformData(viewProjection);
            renderPass.BindVertexBuffer(vertexBuffer);
            renderPass.BindIndexBuffer(indexBuffer, IndexElementSize.ThirtyTwo);
            renderPass.BindFragmentSampler(start.Binding);
            renderPass.DrawIndexedPrimitives(offset * 4u, 0u, (start.Count - offset) * 2u, 1);

            offset += start.Count;
            start = ref Unsafe.Add(ref start, 1);
        }

        rendered = true;
        VertexIndex = 0;
    }

    public void BindPipeline(GraphicsPipeline pipeline)
    {
        queues[onQueue].Pipeline = pipeline;
    }

    internal void ResizeBuffer()
    {
        transferComputeBuffer.Unmap();
        uint maxTextures = (uint)(CurrentMaxTexture += 2048);

        indexBuffer.Dispose();
        indexBuffer = GenerateIndexArray(device, (uint)(maxTextures * 6));

        vertexBuffer.Dispose();
        vertexBuffer = GpuBuffer.Create<PositionTextureColorVertex>(
            device, 
            BufferUsageFlags.Vertex 
            | BufferUsageFlags.ComputeStorageRead 
            | BufferUsageFlags.ComputeStorageWrite, maxTextures * 4);

        transferComputeBuffer.Dispose();
        transferComputeBuffer = TransferBuffer.Create<ComputeData>(device, TransferBufferUsage.Upload, maxTextures);

        computeBuffer.Dispose();
        computeBuffer = GpuBuffer.Create<ComputeData>(device, BufferUsageFlags.ComputeStorageRead, maxTextures);
        CurrentMaxTexture = maxTextures;
    }

    /// <summary>
    /// Dispose all of the <see cref="Riateu.Graphics.Batch"/> resources.
    /// </summary>
    /// <param name="disposing">Dispose all of the native resource</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                vertexBuffer.Dispose();
                indexBuffer.Dispose();
            }

            IsDisposed = true;
        }
    }

    ///
    ~Batch()
    {
#if DEBUG
        Logger.LogWarn($"The type {this.GetType()} has not been disposed properly.");
#endif
        Dispose(disposing: false);
    }

    /// <summary>
    /// Dispose all of the <see cref="Riateu.Graphics.Batch"/> resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        System.GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public void Draw(Quad quad, Vector2 position, Color color, float layerDepth = 1)
    {
        Draw(quad, position, color, Vector2.One, Vector2.Zero, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Quad quad, Vector2 position, Color color, Vector2 scale, float layerDepth = 1)
    {
        Draw(quad, position, color, scale, Vector2.Zero, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Vector2 position, Color color, Vector2 scale, Vector2 origin, float layerDepth = 1)
    {
        Draw(new Quad(queues[onQueue].Binding.Texture), position, color, scale, origin, 0, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Vector2 position, Color color, float layerDepth = 1)
    {
        Draw(new Quad(queues[onQueue].Binding.Texture), position, color, Vector2.One, Vector2.Zero, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Quad quad, Vector2 position, Color color, Vector2 scale, Vector2 origin, float layerDepth = 1)
    {
        Draw(quad, position, color, scale, origin, 0, layerDepth);
    }

    /// <inheritdoc/>
    public unsafe void Draw(Quad quad, Vector2 position, Color color, Vector2 scale, Vector2 origin, float rotation, float layerDepth = 1)
    {
#if DEBUG
        AssertDoesBegin();
#endif
        if (VertexIndex == CurrentMaxTexture)
        {
            ResizeBuffer();
            return;
        }
        computes[VertexIndex] = new ComputeData 
        {
            Position = position,
            Scale = scale,
            Origin = origin,
            UV = new UV(quad.UV[0], quad.UV[1], quad.UV[2], quad.UV[3]),
            Dimension = new Vector2(quad.Source.W, quad.Source.H),
            Rotation = rotation,
            Color = color.ToVector4(),
        };

        VertexIndex++;
    }

#if DEBUG
    private void AssertBegin()
    {
        if (!DEBUG_begin)
            return;
        
        throw new System.Exception("Batch has already begun. End should be called before starting another one.");
    }

    private void AssertRender()
    {
        if (!DEBUG_begin)
            return;
        
        throw new System.Exception("You must End the batch first before rendering.");
    }

    private void AssertDoesBegin() 
    {
        if (DEBUG_begin)
            return;
        
        throw new System.Exception("Batch has not begun yet, please call Begin first.");
    }
#endif


    [StructLayout(LayoutKind.Explicit, Size = 80)]
    private struct ComputeData 
    {
        [FieldOffset(0)]
        public Vector2 Position;
        [FieldOffset(8)]
        public Vector2 Scale;
        [FieldOffset(16)]
        public Vector2 Origin;
        [FieldOffset(24)]
        public UV UV;
        [FieldOffset(56)]
        public Vector2 Dimension;
        [FieldOffset(64)]
        public float Rotation;
        [FieldOffset(80)]
        public Vector4 Color;
    }
}
