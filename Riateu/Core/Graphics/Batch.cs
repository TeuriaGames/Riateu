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
public class Batch : System.IDisposable, IBatch
{

    private struct SubBatch 
    {
        public TextureSamplerBinding Binding;
        public uint Offset;
        public uint Count;
    }
    private const int MaxTextures = 8192;
    private const int MaxSubBatchCount = 8;
    private GraphicsDevice device;
    private PositionTextureColorVertex[] vertices;
    private uint[] indices;
    private SubBatch[] batches = new SubBatch[MaxSubBatchCount];
    private uint batchIndex;
    private uint vertexIndex;
    private uint currentMaxTexture = MaxTextures;

    private GpuBuffer vertexBuffer;
    private GpuBuffer indexBuffer;
    private Stack<Matrix4x4> Matrices;

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
        vertices = new PositionTextureColorVertex[MaxTextures * 4];
        indices = GenerateIndexArray(MaxTextures * 6);

        vertexBuffer = GpuBuffer.Create<PositionTextureColorVertex>(device, BufferUsageFlags.Vertex, (uint)vertices.Length);
        indexBuffer = GpuBuffer.Create<uint>(device, BufferUsageFlags.Index, (uint)indices.Length);

        var view = Matrix4x4.CreateTranslation(0, 0, 0);
        var projection = Matrix4x4.CreateOrthographicOffCenter(0, width, 0, height, -1, 1);
        Matrix = view * projection;
    }

    private static uint[] GenerateIndexArray(uint maxIndices)
    {
        var result = new uint[maxIndices];
        for (uint i = 0, j = 0; i < maxIndices; i += 6, j += 4)
        {
            result[i] = j;
            result[i + 1] = j + 1;
            result[i + 2] = j + 2;
            result[i + 3] = j + 2;
            result[i + 4] = j + 1;
            result[i + 5] = j + 3;
        }

        return result;
    }

    /// <inheritdoc/>
    public void Start()
    {
        batchIndex = 0;
    }
    /// <inheritdoc/>
    public void End(CommandBuffer cmdBuf)
    {
        FlushVertex(cmdBuf);
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
    public void FlushVertex(CommandBuffer cmdBuf) 
    {
        if (vertexIndex == 0) 
        {
            return;
        }

        cmdBuf.SetBufferData(indexBuffer, indices, 0, 0, vertexIndex * 6);
        cmdBuf.SetBufferData(vertexBuffer, vertices, 0, 0, vertexIndex * 4);
        batches[batchIndex].Count = vertexIndex;
    }

    /// <inheritdoc/>
    public void Draw(CommandBuffer cmdBuf) 
    {
        Draw(cmdBuf, Matrix);
    }

    /// <inheritdoc/>
    public void Draw(CommandBuffer cmdBuf, Matrix4x4 viewProjection) 
    {
        if (vertexIndex == 0) 
        {
            return;
        }
        var vertexOffset = cmdBuf.PushVertexShaderUniforms(viewProjection);
        cmdBuf.BindVertexBuffers(vertexBuffer);
        cmdBuf.BindIndexBuffer(indexBuffer, IndexElementSize.ThirtyTwo);

        for (uint i = 0; i < batchIndex + 1; i++) 
        {
            var batch = batches[i];

            cmdBuf.BindFragmentSamplers(batch.Binding);
            cmdBuf.DrawIndexedPrimitives(batch.Offset * 4u, 0u, (batch.Count - batch.Offset) * 2u, vertexOffset, 0u);   
        }

        batchIndex = 0;
        vertexIndex = 0;
    }

    /// <inheritdoc/>
    public void Add(
        Texture texture, Sampler sampler, Vector2 position, Color color, Matrix3x2 transform, float layerDepth = 1) 
    {
        Add(new SpriteTexture(texture), texture, sampler, position, color, transform, layerDepth);
    }

    /// <inheritdoc/>
    public void Add(
        SpriteTexture sTexture, Texture texture, Sampler sampler, Vector2 position, Color color, Matrix3x2 transform,
        float layerDepth = 1) 
    {
        if (texture.IsDisposed) 
        {
            throw new System.ObjectDisposedException(nameof(texture));
        }

        if (vertexIndex > 0 && 
            (texture.Handle != batches[batchIndex].Binding.Texture.Handle || 
            sampler.Handle != batches[batchIndex].Binding.Sampler.Handle)) 
        {
            batches[batchIndex].Count = vertexIndex;
            batchIndex++;

            if (batchIndex == batches.Length) 
            {
                System.Array.Resize(ref batches, batches.Length + MaxSubBatchCount);
            }

            batches[batchIndex].Offset = vertexIndex;
            batches[batchIndex].Binding = new TextureSamplerBinding(texture, sampler);
        }

        if (vertexIndex == 0) 
        {
            batches[batchIndex].Binding = new TextureSamplerBinding(texture, sampler);
        }

        if (vertexIndex == currentMaxTexture) 
        {
            int maxTextures = (int)(currentMaxTexture += 2048);
            System.Array.Resize(ref vertices, vertices.Length + maxTextures * 4);

            indices = GenerateIndexArray((uint)(maxTextures * 6));

            vertexBuffer.Dispose();
            vertexBuffer = Buffer.Create<PositionTextureColorVertex>(
                device, BufferUsageFlags.Vertex, (uint)vertices.Length);

            indexBuffer.Dispose();
            indexBuffer = Buffer.Create<uint>(
                device, BufferUsageFlags.Index, (uint)vertices.Length
            );
        }

        float width = sTexture.Source.W;
        float height = sTexture.Source.H;
        
        var topLeft = new Vector2(position.X, position.Y);
        var topRight = new Vector2(position.X + width, position.Y);
        var bottomLeft = new Vector2(position.X, position.Y + height);
        var bottomRight = new Vector2(position.X + width, position.Y + height);

        var vertexOffset = vertexIndex * 4;

        vertices[vertexOffset].Position = new Vector3(Vector2.Transform(topLeft, transform), layerDepth);
        vertices[vertexOffset + 1].Position = new Vector3(Vector2.Transform(bottomLeft, transform), layerDepth);
        vertices[vertexOffset + 2].Position = new Vector3(Vector2.Transform(topRight, transform), layerDepth);
        vertices[vertexOffset + 3].Position = new Vector3(Vector2.Transform(bottomRight, transform), layerDepth);

        vertices[vertexOffset].Color = color;
        vertices[vertexOffset + 1].Color = color;
        vertices[vertexOffset + 2].Color = color;
        vertices[vertexOffset + 3].Color = color;

        vertices[vertexOffset].TexCoord = sTexture.UV.TopLeft;
        vertices[vertexOffset + 1].TexCoord = sTexture.UV.BottomLeft;
        vertices[vertexOffset + 2].TexCoord = sTexture.UV.TopRight;
        vertices[vertexOffset + 3].TexCoord = sTexture.UV.BottomRight;

        vertexIndex++;
    }

    /// <summary>
    /// Add a canvas texture to the vertex buffer
    /// </summary>
    /// <param name="canvas">A <see cref="Riateu.Canvas"/> to submit with</param>
    /// <param name="sampler">A sampler for acanvas</param>
    public void AddCanvas(Canvas canvas, Sampler sampler) 
    {
        Add(canvas.CanvasTexture, sampler, Vector2.Zero, Color.White, Matrix3x2.Identity);
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
}
