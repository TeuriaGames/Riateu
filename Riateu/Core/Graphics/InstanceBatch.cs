using System.Collections.Generic;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

internal struct SubInstanceBatch 
{
    public Buffer InstancedBuffer;
    public uint InstanceCount;
    public TextureSamplerBinding Binding;
}

/// <summary>
/// A batch system used to create many instances in one draw calls in a same vertex and index buffer 
/// while it can. This also utilizes a texture swapping which would add additional sub instance 
/// batches to be able to draw multiple textures.
/// </summary>
public class InstanceBatch : System.IDisposable, IBatch
{
    private const int MaxInstances = 8192;
    private const int MaxSubBatchCount = 8;
    private GraphicsDevice device;
    private InstancedVertex[] instances;
    private uint instanceCount;
    private uint batchIndex;

    private Buffer vertexBuffer;
    private Buffer indexBuffer;
    private Stack<Matrix4x4> Matrices;
    private SubInstanceBatch[] batches = new SubInstanceBatch[MaxSubBatchCount];

    /// <summary>
    /// A current matrix projection to be used for rendering.
    /// </summary>
    public Matrix4x4 Matrix;
    /// <summary>
    /// A check if the <see cref="Riateu.Graphics.InstanceBatch"/> is already been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// An initialization for the batch system.
    /// </summary>
    /// <param name="device">An application graphics device</param>
    /// <param name="width">A width of a orthographic matrix</param>
    /// <param name="height">A height of a orthographic matrix</param>
    public InstanceBatch(GraphicsDevice device, int width, int height) 
    {
        Matrices = new();
        this.device = device;
        instances = new InstancedVertex[MaxInstances];

        vertexBuffer = Buffer.Create<PositionVertex>(device, BufferUsageFlags.Vertex, 4);
        indexBuffer = Buffer.Create<uint>(device, BufferUsageFlags.Index, 6);

        var view = Matrix4x4.CreateTranslation(0, 0, 0);
        var projection = Matrix4x4.CreateOrthographicOffCenter(0, width, 0, height, -1, 1);
        Matrix = view * projection;

        CommandBuffer buffer = device.AcquireCommandBuffer();

        buffer.SetBufferData<PositionVertex>(vertexBuffer, [
            new (new Vector3(0, 0, 0)),
            new (new Vector3(0, 1, 0)),
            new (new Vector3(1, 0, 0)),
            new (new Vector3(1, 1, 0)),
        ]);

        buffer.SetBufferData<ushort>(indexBuffer, [
            0, 1, 2, 2, 1, 3
        ]);

        device.Submit(buffer);
    }

    /// <inheritdoc/>
    public void Start()
    {
        batchIndex = 0;
    }

    /// <inheritdoc/>
    public void End(CommandBuffer buffer)
    {
        FlushVertex(buffer);
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

    /// <summary>
    /// Send the instances buffer to the GPU.
    /// </summary>
    /// <param name="cmdBuf">
    /// A <see cref="MoonWorks.Graphics.CommandBuffer"/> for sending the vertex buffer to the GPU
    /// </param>
    public void FlushVertex(CommandBuffer cmdBuf) 
    {
        if (instanceCount == 0) 
        {
            return;
        }
        batches[batchIndex].InstanceCount = instanceCount;
        if (batches[batchIndex].InstancedBuffer == null) 
        {
            batches[batchIndex].InstancedBuffer = Buffer.Create<InstancedVertex>(device, BufferUsageFlags.Vertex, (uint)instances.Length);
        }

        cmdBuf.SetBufferData(batches[batchIndex].InstancedBuffer, instances, 0, 0, instanceCount);

        instanceCount = 0;
    }

    /// <summary>
    /// Draw the vertex and all instances into the screen.
    /// </summary>
    /// <param name="cmdBuf">
    /// A <see cref="MoonWorks.Graphics.CommandBuffer"/> to create a render pass and bind
    /// all of the buffers and uniforms.
    /// </param>
    public void Draw(CommandBuffer cmdBuf) 
    {
        Draw(cmdBuf, Matrix);
    }

    /// <summary>
    /// Draw the vertex and all instances into the screen with a custom view projection.
    /// </summary>
    /// <param name="cmdBuf">
    /// A <see cref="MoonWorks.Graphics.CommandBuffer"/> to create a render pass and bind
    /// all of the buffers and uniforms.
    /// </param>
    /// <param name="viewProjection">A 4x4 matrix to project on screen</param>
    public void Draw(CommandBuffer cmdBuf, Matrix4x4 viewProjection) 
    {
        if (batches[0].InstanceCount == 0)
            return;
        var vertexOffset = cmdBuf.PushVertexShaderUniforms(viewProjection);

        for (int i = 0; i < batchIndex + 1; i++) 
        {
            var batch = batches[i];
            cmdBuf.BindVertexBuffers(vertexBuffer, batch.InstancedBuffer);
            cmdBuf.BindIndexBuffer(indexBuffer, IndexElementSize.Sixteen);

            cmdBuf.BindFragmentSamplers(batch.Binding);
            cmdBuf.DrawInstancedPrimitives(0u, 0u, 2u, batch.InstanceCount, vertexOffset, 0u);
        }

        batchIndex = 0;
    }

    /// <summary>
    /// Adds an instance data to a batch
    /// </summary>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="transform">A transform matrix</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    public void Add(
        Texture baseTexture, 
        Sampler sampler, 
        Vector2 position, 
        Color color,
        Matrix3x2 transform, 
        float layerDepth = 1) 
    {
        Add(new SpriteTexture(baseTexture), baseTexture, sampler, position, color, transform, layerDepth);
    }

    /// <summary>
    /// Adds an instance data to a batch
    /// </summary>
    /// <param name="sTexture">A spriteTexture to set a quad and coords for the texture</param>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="transform">A transform matrix</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    public void Add(
        SpriteTexture sTexture, 
        Texture baseTexture, 
        Sampler sampler, 
        Vector2 position, 
        Color color,
        Matrix3x2 transform, 
        float layerDepth = 1) 
    {
        if (instanceCount > 0 && (
            baseTexture.Handle != batches[batchIndex].Binding.Texture.Handle ||
            sampler.Handle != batches[batchIndex].Binding.Sampler.Handle
        )) 
        {
            CommandBuffer cmdBuf = device.AcquireCommandBuffer();
            FlushVertex(cmdBuf);
            device.Submit(cmdBuf);

            batchIndex++;
            if (batchIndex >= batches.Length) 
            {
                System.Array.Resize(ref batches, batches.Length + MaxSubBatchCount);
            }
            batches[batchIndex].Binding = new TextureSamplerBinding(baseTexture, sampler);
        }

        if (instanceCount == 0) 
        {
            batches[batchIndex].Binding = new TextureSamplerBinding(baseTexture, sampler);
        }

        if (instanceCount == instances.Length) 
        {
            int maxInstances = (int)(instanceCount + 2048);
            System.Array.Resize(ref instances, maxInstances);
        }
        
        instances[instanceCount] = new InstancedVertex(
            new Vector3(Vector2.Transform(position, transform), layerDepth),
            new Vector2(sTexture.Width, sTexture.Height),
            sTexture.UV,
            color
        );
        instanceCount++;
    }

    /// <summary>
    /// Dispose all of the <see cref="Riateu.Graphics.InstanceBatch"/> resources.
    /// </summary>
    /// <param name="disposing">Dispose all of the native resource</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                for (int i = 0; i < batches.Length; i++) 
                {
                    batches[i].InstancedBuffer.Dispose();
                }
                vertexBuffer.Dispose();
                indexBuffer.Dispose();
            }

            IsDisposed = true;
        }
    }

    /// 
    ~InstanceBatch()
    {
#if DEBUG
        Logger.LogWarn($"The type {this.GetType()} has not been disposed properly.");
#endif
        Dispose(disposing: false);
    }

    /// <summary>
    /// Dispose all of the <see cref="Riateu.Graphics.InstanceBatch"/> resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        System.GC.SuppressFinalize(this);
    }
}