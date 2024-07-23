using GpuBuffer = MoonWorks.Graphics.Buffer;
using MoonWorks;
using MoonWorks.Graphics;
using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;


/// <summary>
/// A batch system used to batch all of the vertices in one draw calls while it can.
/// This also utilizes a texture swapping which would add additional sub batches to be
/// able to draw multiple textures.
/// </summary>
public class Batch : System.IDisposable, IRenderable
{
    private const uint MaxTextures = 4096;
    private const uint InitialMaxQueues = 4;
    private GraphicsDevice device;
    private unsafe ComputeData* computes;

    private bool rendered;

    private GpuBuffer vertexBuffer;
    private GpuBuffer indexBuffer;
    private GpuBuffer computeBuffer;
    private TransferBuffer transferComputeBuffer;
    private BatchQueue[] queues = new BatchQueue[InitialMaxQueues];
    private uint onQueue = uint.MaxValue;

#if DEBUG
    private bool DEBUG_begin;
    private bool DEBUG_isFlushed;
#endif
    private uint vertexIndex;
    private uint currentMaxTexture = MaxTextures;


    /// <summary>
    /// A default matrix projection to be used for rendering.
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

    /// <summary>
    /// Starts a new batch.
    /// </summary>
    /// <param name="texture">A texture to be used in the slot</param>
    /// <param name="sampler">A sampler to used for the texture</param>
    public void Begin(Texture texture, Sampler sampler)
    {
        Begin(texture, sampler, GameContext.DefaultMaterial, Matrix);
    }

    /// <summary>
    /// Starts a new batch.
    /// </summary>
    /// <param name="texture">A texture to be used in the slot</param>
    /// <param name="sampler">A sampler to used for the texture</param>
    /// <param name="transform">A transformation matrix</param>
    public void Begin(Texture texture, Sampler sampler, Matrix4x4 transform)
    {
        Begin(texture, sampler, GameContext.DefaultMaterial, transform);
    }

    /// <summary>
    /// Starts a new batch.
    /// </summary>
    /// <param name="texture">A texture to be used in the slot</param>
    /// <param name="sampler">A sampler to used for the texture</param>
    /// <param name="material">A shader material to use</param>
    public void Begin(Texture texture, Sampler sampler, Material material)
    {
        Begin(texture, sampler, material, Matrix);
    }

    /// <summary>
    /// Starts a new batch.
    /// </summary>
    /// <param name="texture">A texture to be used in the slot</param>
    /// <param name="sampler">A sampler to used for the texture</param>
    /// <param name="material">A shader material to use</param>
    /// <param name="transform">A transformation matrix</param>
    public void Begin(Texture texture, Sampler sampler, Material material, Matrix4x4 transform)
    {
#if DEBUG
        AssertBegin();
        DEBUG_begin = true;
#endif
        if (rendered)
        {
            vertexIndex = 0;
            onQueue = uint.MaxValue;
            rendered = false;
        }

        unchecked { onQueue++; }

        if (queues.Length == onQueue) 
        {
            Array.Resize(ref queues, queues.Length + 4);
        }

        queues[onQueue] = new BatchQueue 
        {
            Binding = new TextureSamplerBinding(texture, sampler),
            Material = material,
            Count = 0,
            Offset = vertexIndex,
            Matrix = transform
        };

        unsafe {
            transferComputeBuffer.Map(true, out byte* data);
            computes = (ComputeData*)data;
        }
    }

    /// <summary>
    /// End the existing batch, must render before starting a new one.
    /// <param name="flush">Whether to flush it already and decide not go further on.</param>
    /// </summary>
    public void End(bool flush)
    {
#if DEBUG
        AssertDoesBegin();
        DEBUG_begin = false;
#endif
        transferComputeBuffer.Unmap();

        if (vertexIndex == 0)
        {
            return;
        }

        var offset = queues[onQueue].Offset;
        queues[onQueue].Count = vertexIndex - offset;

        if (flush) 
        {
            Flush();
        }
    }

    /// <inheritdoc/>
    private void BindUniformMatrix(in Matrix4x4 matrix)
    {
        GraphicsExecutor.Executor.PushVertexUniformData<Matrix4x4>(matrix, 0);
    }

    public void Flush() 
    {
#if DEBUG
        if (DEBUG_isFlushed) 
        {
            Logger.LogWarn("The state has been flushed, yet has been flushed again. Avoid doing this everytime as it cost performance.");
        }
        DEBUG_isFlushed = true;
#endif
        CommandBuffer cmdBuf = GraphicsExecutor.Executor;

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
        computePass.Dispatch(currentMaxTexture / 64, 1, 1);

        cmdBuf.EndComputePass(computePass);
    }

    /// <inheritdoc/>
    public void Render(RenderPass renderPass)
    {
#if DEBUG
        AssertIsFlushed();
        AssertRender();
        DEBUG_isFlushed = false;
        DEBUG_begin = false;
#endif

        rendered = true;
        if (vertexIndex == 0)
        {
            return;
        }

        ref var start = ref MemoryMarshal.GetArrayDataReference(queues);
        ref var end = ref Unsafe.Add(ref start, onQueue + 1);

        while (Unsafe.IsAddressLessThan(ref start, ref end)) 
        {
            VertexUniformBinder binder = new VertexUniformBinder();
            BindUniformMatrix(start.Matrix);
            renderPass.BindGraphicsPipeline(start.Material.ShaderPipeline);
            start.Material.BindUniforms(binder);
            renderPass.BindVertexBuffer(vertexBuffer);
            renderPass.BindIndexBuffer(indexBuffer, IndexElementSize.ThirtyTwo);
            renderPass.BindFragmentSampler(start.Binding);
            renderPass.DrawIndexedPrimitives(start.Offset * 4u, 0u, start.Count * 2u, 1);

            start = ref Unsafe.Add(ref start, 1);
        }

        vertexIndex = 0;
    }

    internal void ResizeBuffer()
    {
        transferComputeBuffer.Unmap();
        uint maxTextures = (uint)(currentMaxTexture += 2048);

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
        currentMaxTexture = maxTextures;
        unsafe {
            transferComputeBuffer.Map(true, out byte* data);
            computes = (ComputeData*)data;
        }
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

    /// <summary>
    /// Draw a texture based on the current quad and submit it in the current batch.
    /// </summary>
    /// <param name="quad">A quad or coordinates of the texture</param>
    /// <param name="position">A position to where it should draw</param>
    /// <param name="color">A color mask</param>
    /// <param name="layerDepth">A depth of the layer of the drawn texture</param>
    public void Draw(TextureQuad quad, Vector2 position, Color color, float layerDepth = 1)
    {
        Draw(quad, position, color, Vector2.One, Vector2.Zero, layerDepth);
    }

    /// <summary>
    /// Draw a texture based on the current quad and submit it in the current batch.
    /// </summary>
    /// <param name="quad">A quad or coordinates of the texture</param>
    /// <param name="position">A position to where it should draw</param>
    /// <param name="color">A color mask</param>
    /// <param name="scale">A scale of the drawn texture</param>
    /// <param name="layerDepth">A depth of the layer of the drawn texture</param>
    /// <inheritdoc/>
    public void Draw(TextureQuad quad, Vector2 position, Color color, Vector2 scale, float layerDepth = 1)
    {
        Draw(quad, position, color, scale, Vector2.Zero, layerDepth);
    }

    /// <summary>
    /// Draw a texture and submit it in the current batch.
    /// </summary>
    /// <param name="position">A position to where it should draw</param>
    /// <param name="color">A color mask</param>
    /// <param name="scale">A scale of the drawn texture</param>
    /// <param name="origin">An origin or offset of the drawn texture</param>
    /// <param name="layerDepth">A depth of the layer of the drawn texture</param>
    /// <inheritdoc/>
    public void Draw(Vector2 position, Color color, Vector2 scale, Vector2 origin, float layerDepth = 1)
    {
        Draw(new TextureQuad(queues[onQueue].Binding.Texture), position, color, scale, origin, 0, layerDepth);
    }

    /// <summary>
    /// Draw a texture and submit it in the current batch.
    /// </summary>
    /// <param name="position">A position to where it should draw</param>
    /// <param name="color">A color mask</param>
    /// <param name="layerDepth">A depth of the layer of the drawn texture</param>
    public void Draw(Vector2 position, Color color, float layerDepth = 1)
    {
        Draw(new TextureQuad(queues[onQueue].Binding.Texture), position, color, Vector2.One, Vector2.Zero, layerDepth);
    }

    /// <summary>
    /// Draw a texture based on the current quad and submit it in the current batch.
    /// </summary>
    /// <param name="quad">A quad or coordinates of the texture</param>
    /// <param name="position">A position to where it should draw</param>
    /// <param name="color">A color mask</param>
    /// <param name="scale">A scale of the drawn texture</param>
    /// <param name="origin">An origin or offset of the drawn texture</param>
    /// <param name="layerDepth">A depth of the layer of the drawn texture</param>
    public void Draw(TextureQuad quad, Vector2 position, Color color, Vector2 scale, Vector2 origin, float layerDepth = 1)
    {
        Draw(quad, position, color, scale, origin, 0, layerDepth);
    }

    /// <summary>
    /// Draw a texture based on the current quad and submit it in the current batch.
    /// </summary>
    /// <param name="quad">A quad or coordinates of the texture</param>
    /// <param name="position">A position to where it should draw</param>
    /// <param name="color">A color mask</param>
    /// <param name="scale">A scale of the drawn texture</param>
    /// <param name="origin">An origin or offset of the drawn texture</param>
    /// <param name="rotation">A rotation of the drawn texture</param>
    /// <param name="layerDepth">A depth of the layer of the drawn texture</param>
    public unsafe void Draw(TextureQuad quad, Vector2 position, Color color, Vector2 scale, Vector2 origin, float rotation, float layerDepth = 1)
    {
#if DEBUG
        AssertDoesBegin();
#endif
        if (vertexIndex == currentMaxTexture)
        {
            ResizeBuffer();
        }
        computes[vertexIndex] = new ComputeData 
        {
            Position = position,
            Scale = scale,
            Origin = origin,
            UV = new UV(quad.UV[0], quad.UV[1], quad.UV[2], quad.UV[3]),
            Dimension = new Vector2(quad.Source.W, quad.Source.H),
            Rotation = rotation,
            Depth = layerDepth,
            Color = color.ToVector4(),
        };

        vertexIndex++;
    }

    /// <summary>
    /// Draw the text and submit its quad in the current batch.
    /// </summary>
    /// <param name="font">A font to use</param>
    /// <param name="text">A text to be drawn</param>
    /// <param name="position">A position of the text</param>
    /// <param name="color">A color of the text</param>
    /// <param name="hAlignment">A horizontal alignment of the text</param>
    public void Draw(SpriteFont font, string text, Vector2 position, Color color, FontAlignment hAlignment = FontAlignment.Baseline) 
    {
        Draw(font, text, position, color, Vector2.One, hAlignment);
    }

    /// <summary>
    /// Draw the text and submit its quad in the current batch.
    /// </summary>
    /// <param name="font">A font to use</param>
    /// <param name="text">A text to be drawn</param>
    /// <param name="position">A position of the text</param>
    /// <param name="color">A color of the text</param>
    /// <param name="scale">A scale of the text when drawn</param>
    /// <param name="hAlignment">A horizontal alignment of the text</param>
    public void Draw(SpriteFont font, string text, Vector2 position, Color color, Vector2 scale, FontAlignment hAlignment = FontAlignment.Baseline) 
    {
#if DEBUG
        AssertDoesBegin();
#endif
        if (vertexIndex == currentMaxTexture)
        {
            ResizeBuffer();
        }

        Vector2 justify = hAlignment switch {
            FontAlignment.Baseline => new Vector2(0, 0),
            FontAlignment.Center => new Vector2(0.5f, 0),
            FontAlignment.End => new Vector2(1, 0),
            _ => throw new NotImplementedException()
        };

        unsafe {
            font.Draw(computes, ref vertexIndex, text, position, justify, color, scale);
        }
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

    private void AssertIsFlushed() 
    {
        if (DEBUG_isFlushed)
            return;
        
        throw new System.Exception("Batch has not been flushed yet. You might need to set true on the End(true) or call .Flush()");
    }
#endif
    private struct BatchQueue 
    {
        public uint Count;
        public uint Offset;
        public TextureSamplerBinding Binding;
        public Material Material;
        public Matrix4x4 Matrix;
    }

    [StructLayout(LayoutKind.Explicit, Size = 96)]
    internal struct ComputeData 
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
        [FieldOffset(68)]
        public float Depth;
        [FieldOffset(80)]
        public Vector4 Color;
    }
}


