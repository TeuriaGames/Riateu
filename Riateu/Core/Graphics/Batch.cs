using GpuBuffer = MoonWorks.Graphics.Buffer;
using System.Collections.Generic;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;


/// <summary>
/// A batch system used to batch all of the vertices in one draw calls while it can.
/// This also utilizes a texture swapping which would add additional sub batches to be
/// able to draw multiple textures.
/// </summary>
public class Batch : System.IDisposable
{
    private const int MaxTextures = 8192;
    private GraphicsDevice device;
    private unsafe PositionTextureColorVertex* vertices;

    private Stack<Matrix4x4> Matrices;
    private bool rendered;

    private GpuBuffer vertexBuffer;
    private GpuBuffer indexBuffer;
    private TransferBuffer transferVertexBuffer;

    public TextureSamplerBinding UsedBinding;
    public GraphicsPipeline UsedPipeline;

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

        vertexBuffer = GpuBuffer.Create<PositionTextureColorVertex>(device, BufferUsageFlags.Vertex, MaxTextures * 4);
        indexBuffer = GenerateIndexArray(device, MaxTextures * 6);

        transferVertexBuffer = new TransferBuffer(device, TransferBufferUsage.Upload, vertexBuffer.Size);

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
        if (rendered)
        {
            VertexIndex = 0;
            rendered = false;
        }

        UsedBinding = new TextureSamplerBinding(texture, sampler);
        UsedPipeline = GameContext.DefaultPipeline;

        unsafe {
            transferVertexBuffer.Map(true, out byte* vert);
            vertices = (PositionTextureColorVertex*)vert;
        }
    }

    /// <inheritdoc/>
    public void End(CommandBuffer cmdBuf)
    {
        transferVertexBuffer.Unmap();
        if (VertexIndex == 0)
        {
            return;
        }
        CopyPass copyPass = cmdBuf.BeginCopyPass();
        copyPass.UploadToBuffer(transferVertexBuffer, vertexBuffer, true);
        cmdBuf.EndCopyPass(copyPass);
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
        if (VertexIndex == 0)
        {
            return;
        }

        renderPass.BindGraphicsPipeline(UsedPipeline);
        renderPass.PushVertexUniformData(viewProjection);
        renderPass.BindVertexBuffer(vertexBuffer);
        renderPass.BindIndexBuffer(indexBuffer, IndexElementSize.ThirtyTwo);
        renderPass.BindFragmentSampler(UsedBinding);
        renderPass.PushFragmentUniformData(fragmentUniform);
        renderPass.DrawIndexedPrimitives(0u, 0u, VertexIndex * 2u, 1);

        VertexIndex = 0;
    }

    /// <inheritdoc/>
    public void Render(RenderPass renderPass, Matrix4x4 viewProjection)
    {
        if (VertexIndex == 0)
        {
            return;
        }

        renderPass.BindGraphicsPipeline(UsedPipeline);
        renderPass.PushVertexUniformData(viewProjection);
        renderPass.BindVertexBuffer(vertexBuffer);
        renderPass.BindIndexBuffer(indexBuffer, IndexElementSize.ThirtyTwo);
        renderPass.BindFragmentSampler(UsedBinding);
        renderPass.DrawIndexedPrimitives(0u, 0u, VertexIndex * 2u, 1);

        VertexIndex = 0;
    }

    public void BindPipeline(GraphicsPipeline pipeline)
    {
        UsedPipeline = pipeline;
    }

    internal void ResizeBuffer()
    {
        transferVertexBuffer.Unmap();
        uint maxTextures = (uint)(CurrentMaxTexture += 2048);

        indexBuffer.Dispose();
        indexBuffer = GenerateIndexArray(device, (uint)(maxTextures * 6));

        vertexBuffer.Dispose();
        vertexBuffer = GpuBuffer.Create<PositionTextureColorVertex>(
            device, BufferUsageFlags.Vertex, maxTextures * 4);

        transferVertexBuffer.Dispose();
        transferVertexBuffer = new TransferBuffer(device, TransferBufferUsage.Upload, vertexBuffer.Size);
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
    public void Draw(Quad quad, Vector2 position, Color color, Matrix3x2 transform, float layerDepth = 1)
    {
        Draw(quad, position, color, Vector2.One, Vector2.Zero, transform, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Quad quad, Vector2 position, Color color, float layerDepth = 1)
    {
        Draw(quad, position, color, Vector2.One, Vector2.Zero, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Quad quad, Vector2 position, Color color, Vector2 scale, Matrix3x2 transform, float layerDepth = 1)
    {
        Draw(quad, position, color, scale, Vector2.Zero, transform, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Quad quad, Vector2 position, Color color, Vector2 scale, float layerDepth = 1)
    {
        Draw(quad, position, color, scale, Vector2.Zero, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Quad quad, Vector2 position, Color color, Vector2 scale, Vector2 origin, Matrix3x2 transform, float layerDepth = 1)
    {
        Draw(quad, position, color, scale, origin, 0, transform, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Quad quad, Vector2 position, Color color, Vector2 scale, Vector2 origin, float layerDepth = 1)
    {
        Draw(quad, position, color, scale, origin, 0, Matrix3x2.Identity, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Vector2 position, Color color, Vector2 scale, Vector2 origin, float layerDepth = 1)
    {
        Draw(new Quad(UsedBinding.Texture), position, color, scale, origin, 0, Matrix3x2.Identity, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Vector2 position, Color color, float layerDepth = 1)
    {
        Draw(new Quad(UsedBinding.Texture), position, color, Vector2.One, Vector2.Zero, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Quad quad, Vector2 position, Color color, Vector2 scale, Vector2 origin, float rotation, float layerDepth = 1)
    {
        Draw(quad, position, color, scale, origin, rotation, Matrix3x2.Identity, layerDepth);
    }

    /// <inheritdoc/>
    public unsafe void Draw(Quad quad, Vector2 position, Color color, Vector2 scale, Vector2 origin, float rotation, Matrix3x2 transform, float layerDepth = 1)
    {
        if (VertexIndex == CurrentMaxTexture)
        {
            ResizeBuffer();
            return;
        }

        float width = quad.Source.W * scale.X;
        float height = quad.Source.H * scale.Y;

        transform = Matrix3x2.CreateTranslation(-origin.X, -origin.Y)
            * Matrix3x2.CreateRotation(rotation)
            * transform;

        var topLeft = new Vector2(position.X, position.Y);
        var topRight = new Vector2(position.X + width, position.Y);
        var bottomLeft = new Vector2(position.X, position.Y + height);
        var bottomRight = new Vector2(position.X + width, position.Y + height);

        var vertexOffset = VertexIndex * 4;

        vertices[vertexOffset].Position = new Vector3(Vector2.Transform(topLeft, transform), layerDepth);
        vertices[vertexOffset + 1].Position = new Vector3(Vector2.Transform(bottomLeft, transform), layerDepth);
        vertices[vertexOffset + 2].Position = new Vector3(Vector2.Transform(topRight, transform), layerDepth);
        vertices[vertexOffset + 3].Position = new Vector3(Vector2.Transform(bottomRight, transform), layerDepth);

        vertices[vertexOffset].Color = color;
        vertices[vertexOffset + 1].Color = color;
        vertices[vertexOffset + 2].Color = color;
        vertices[vertexOffset + 3].Color = color;

        vertices[vertexOffset].TexCoord = quad.UV.TopLeft;
        vertices[vertexOffset + 1].TexCoord = quad.UV.BottomLeft;
        vertices[vertexOffset + 2].TexCoord = quad.UV.TopRight;
        vertices[vertexOffset + 3].TexCoord = quad.UV.BottomRight;

        VertexIndex++;
    }
}
