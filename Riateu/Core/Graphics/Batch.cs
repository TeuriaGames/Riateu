using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;

namespace Riateu.Graphics;


/// <summary>
/// A batch system used to batch all of the vertices in one draw calls while it can.
/// This also utilizes a texture swapping which would add additional sub batches to be
/// able to draw multiple textures.
/// </summary>
public class Batch : System.IDisposable
{
    private const uint MaxTextures = 4096;
    private const uint InitialMaxQueues = 4;
    private GraphicsDevice device;

    private bool rendered;
    private uint onQueue;

    private StructuredBuffer<BatchData> batchBuffer;
    private TransferBuffer transferBatchBuffer;
    private BatchQueue[] queues = new BatchQueue[InitialMaxQueues];

    public StructuredBuffer<BatchData> BatchBuffer => batchBuffer;

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

        transferBatchBuffer = TransferBuffer.Create<BatchData>(device, TransferBufferUsage.Upload, MaxTextures);
        batchBuffer = new StructuredBuffer<BatchData>(device, BufferUsageFlags.ComputeStorageRead | BufferUsageFlags.Vertex, MaxTextures);
        batchBuffer.Name = "BatchComputeBuffer";

        var view = Matrix4x4.CreateTranslation(0, 0, 0);
        var projection = Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);
        Matrix = view * projection;
    }

    /// <summary>
    /// Starts a new batch.
    /// </summary>
    /// <param name="texture">A texture to be used in the slot</param>
    /// <param name="sampler">A sampler to used for the texture</param>
    public void Begin(Texture texture, Sampler sampler)
    {
        Begin(texture, sampler, GameContext.BatchMaterial, Matrix);
    }

    /// <summary>
    /// Starts a new batch.
    /// </summary>
    /// <param name="texture">A texture to be used in the slot</param>
    /// <param name="sampler">A sampler to used for the texture</param>
    /// <param name="transform">A transformation matrix</param>
    public void Begin(Texture texture, Sampler sampler, Matrix4x4 transform)
    {
        Begin(texture, sampler, GameContext.BatchMaterial, transform);
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
            onQueue = 0;
            rendered = false;
        }

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
        transferBatchBuffer.Map(true);
    }

    /// <summary>
    /// End the existing batch, must render before starting a new one.
    /// </summary>
    public void End()
    {
#if DEBUG
        AssertDoesBegin();
        DEBUG_begin = false;

#endif
        transferBatchBuffer.Unmap();

        if (vertexIndex == 0)
        {
            return;
        }

        var offset = queues[onQueue].Offset;
        var count = vertexIndex - offset;
        queues[onQueue].Count = count;

        onQueue++;
    }

    public void Flush(CommandBuffer commandBuffer) 
    {
#if DEBUG
        AssertBegin();
        DEBUG_isFlushed = true;
#endif
        CopyPass copyPass = commandBuffer.BeginCopyPass();
        copyPass.UploadToBuffer(transferBatchBuffer, batchBuffer, true);
        commandBuffer.EndCopyPass(copyPass);
    }

    /// <inheritdoc/>
    public void Render(RenderPass renderPass)
    {
#if DEBUG
        AssertRender();
        AssertIsFlushed();
        DEBUG_isFlushed = false;
        DEBUG_begin = false;
#endif

        rendered = true;
        if (vertexIndex == 0)
        {
            return;
        }

        ref var start = ref MemoryMarshal.GetArrayDataReference(queues);
        ref var end = ref Unsafe.Add(ref start, onQueue);

        while (Unsafe.IsAddressLessThan(ref start, ref end)) 
        {
            renderPass.CommandBuffer.PushVertexUniformData(start.Matrix, 0);
            renderPass.BindGraphicsPipeline(start.Material.ShaderPipeline);
            renderPass.BindStorageVertexBuffer(batchBuffer);
            renderPass.BindFragmentSampler(start.Binding);
            start.Material.BindUniforms(new UniformBinder(renderPass.CommandBuffer));
            renderPass.DrawPrimitives(4, start.Count, 0, start.Offset);

            start = ref Unsafe.Add(ref start, 1);
        }
    }

    internal void ResizeBuffer()
    {
        transferBatchBuffer.Unmap();
        uint maxTextures = (uint)(currentMaxTexture += 2048);

        transferBatchBuffer.Dispose();
        transferBatchBuffer = TransferBuffer.Create<BatchData>(device, TransferBufferUsage.Upload, maxTextures);

        batchBuffer.Dispose();
        batchBuffer = new StructuredBuffer<BatchData>(device, BufferUsageFlags.ComputeStorageRead, maxTextures);
        currentMaxTexture = maxTextures;
        transferBatchBuffer.Map(true);
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
                batchBuffer.Dispose();
                transferBatchBuffer.Dispose();
            }

            IsDisposed = true;
        }
    }

    ///
    ~Batch()
    {
#if DEBUG
        Logger.Warn($"The type {this.GetType()} has not been disposed properly.");
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
        var datas = transferBatchBuffer.MappedTouch<BatchData>();
        datas[(int)vertexIndex] = new BatchData 
        {
            Position = new Vector3(position, layerDepth),
            Scale = new Vector2(quad.Source.Width, quad.Source.Height) * scale,
            Origin = origin * scale,
            UV = new Vector4(quad.UV.TopLeft.X, quad.UV.TopLeft.Y, quad.UV.BottomRight.X, quad.UV.BottomRight.Y),
            Rotation = rotation,
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
    public void Draw(SpriteFont font, string text, Vector2 position, Color color, FontAlignment hAlignment = FontAlignment.Baseline, float layerDepth = 1f) 
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
    public void Draw(SpriteFont font, string text, Vector2 position, Color color, Vector2 scale, FontAlignment hAlignment = FontAlignment.Baseline, float layerDepth = 1f) 
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

        font.Draw(transferBatchBuffer.MappedTouch<BatchData>(), ref vertexIndex, text, position, justify, color, scale, layerDepth);
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

    [StructLayout(LayoutKind.Explicit, Size = 64)]
    public struct BatchData 
    {
        [FieldOffset(0)]
        public Vector4 UV;
        [FieldOffset(16)]
        public Vector3 Position;
        [FieldOffset(28)]
        public float Rotation;
        [FieldOffset(32)]
        public Vector4 Color;
        [FieldOffset(48)]
        public Vector2 Scale;
        [FieldOffset(56)]
        public Vector2 Origin;
    }
}


