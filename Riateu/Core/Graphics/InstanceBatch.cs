using GpuBuffer = MoonWorks.Graphics.Buffer;
using System;
using System.Collections.Generic;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;


/// <summary>
/// A batch system used to create many instances in one draw calls in a same vertex and index buffer
/// while it can. This also utilizes a texture swapping which would add additional sub instance
/// batches to be able to draw multiple textures.
/// </summary>
public unsafe class InstanceBatch : System.IDisposable, IBatch
{
    private struct SubInstanceBatch
    {
        public GraphicsPipeline GraphicsPipeline;
        public GpuBuffer InstancedBuffer;
        public uint InstanceCount;
        public TextureSamplerBinding Binding;
    }
    private const int MaxInstances = 4096;
    private const int MaxSubBatchCount = 4;
    private GraphicsDevice device;
    private uint instancesSize = MaxInstances;
    private uint instanceCount;
    private uint batchIndex;

    private GpuBuffer vertexBuffer;
    private GpuBuffer indexBuffer;
    private TransferBuffer transferBuffer;
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

        transferBuffer = TransferBuffer.Create<InstancedVertex>(device, TransferBufferUsage.Upload, MaxInstances);

        var view = Matrix4x4.CreateTranslation(0, 0, 0);
        var projection = Matrix4x4.CreateOrthographicOffCenter(0, width, 0, height, -1, 1);
        Matrix = view * projection;


        CommandBuffer buffer = device.AcquireCommandBuffer();

        using var resourceUploader = new ResourceUploader(device);

        Span<PositionVertex> vertices = [
            new (new Vector3(0, 0, 0)),
            new (new Vector3(0, 1, 0)),
            new (new Vector3(1, 0, 0)),
            new (new Vector3(1, 1, 0)),
        ];

        Span<ushort> indices = [0, 1, 2, 2, 1, 3];

        vertexBuffer = resourceUploader.CreateBuffer<PositionVertex>(vertices, BufferUsageFlags.Vertex);
        indexBuffer = resourceUploader.CreateBuffer<ushort>(indices, BufferUsageFlags.Index);
        resourceUploader.Upload();
    }

    /// <inheritdoc/>
    public void Begin()
    {
        batchIndex = 0;
        batches[batchIndex].GraphicsPipeline = GameContext.InstancedPipeline;
    }

    /// <summary>
    /// Send the instances buffer to the GPU.
    /// </summary>
    /// <param name="cmdBuf">
    /// A <see cref="MoonWorks.Graphics.CommandBuffer"/> for sending the vertex buffer to the GPU
    /// </param>
    public void End(CommandBuffer cmdBuf)
    {
        if (instanceCount == 0)
        {
            return;
        }
        batches[batchIndex].InstanceCount = instanceCount;
        if (batches[batchIndex].InstancedBuffer == null)
        {
            batches[batchIndex].InstancedBuffer = GpuBuffer.Create<InstancedVertex>(device, BufferUsageFlags.Vertex, instancesSize);
        }

        CopyPass copyPass = cmdBuf.BeginCopyPass();
        copyPass.UploadToBuffer(transferBuffer, batches[batchIndex].InstancedBuffer, true);
        cmdBuf.EndCopyPass(copyPass);

        instanceCount = 0;
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
    /// Draw the vertex and all instances into the screen.
    /// </summary>
    /// <param name="renderPass">
    /// A <see cref="MoonWorks.Graphics.RenderPass"/> to bind
    /// all of the buffers and uniforms.
    /// </param>
    public void Draw(RenderPass renderPass)
    {
        Draw(renderPass, Matrix);
    }

    /// <summary>
    /// Draw the vertex and all instances into the screen with a custom view projection.
    /// </summary>
    /// <param name="renderPass">
    /// A <see cref="MoonWorks.Graphics.RenderPass"/> to bind
    /// all of the buffers and uniforms.
    /// </param>
    /// <param name="viewProjection">A 4x4 matrix to project on screen</param>
    public void Draw(RenderPass renderPass, Matrix4x4 viewProjection)
    {
        if (batches[0].InstanceCount == 0)
            return;

        for (int i = 0; i < batchIndex + 1; i++)
        {
            var batch = batches[i];

            renderPass.BindGraphicsPipeline(batch.GraphicsPipeline);
            renderPass.PushVertexUniformData(viewProjection);
            renderPass.BindVertexBuffer(vertexBuffer);
            renderPass.BindVertexBuffer(batch.InstancedBuffer, 1);
            renderPass.BindIndexBuffer(indexBuffer, IndexElementSize.Sixteen);

            renderPass.BindFragmentSampler(batch.Binding);
            renderPass.DrawIndexedPrimitives(0u, 0u, 2u, batch.InstanceCount);
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
        Add(new Quad(baseTexture), baseTexture, sampler, position, color, Vector2.One, Vector2.Zero, 0, transform, layerDepth);
    }

    /// <summary>
    /// Adds an instance data to a batch
    /// </summary>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    public void Add(
        Texture baseTexture,
        Sampler sampler,
        Vector2 position,
        Color color,
        float layerDepth = 1)
    {
        Add(new Quad(baseTexture), baseTexture, sampler, position, color, Vector2.One, Vector2.Zero, 0, layerDepth);
    }

    /// <summary>
    /// Adds an instance data to a batch
    /// </summary>
    /// <param name="quad">A spriteTexture to set a quad and coords for the texture</param>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="transform">A transform matrix</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    public void Add(
        Quad quad,
        Texture baseTexture,
        Sampler sampler,
        Vector2 position,
        Color color,
        Matrix3x2 transform,
        float layerDepth = 1)
    {
        Add(quad, baseTexture, sampler, Vector2.Transform(position, transform), color, Vector2.One, Vector2.Zero, 0, layerDepth);
    }

    /// <summary>
    /// Adds an instance data to a batch
    /// </summary>
    /// <param name="quad">A spriteTexture to set a quad and coords for the texture</param>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    public void Add(Quad quad, Texture baseTexture, Sampler sampler, Vector2 position, Color color, float layerDepth = 1)
    {
        Add(quad, baseTexture, sampler, position, color, Vector2.One, Vector2.Zero, 0, layerDepth);
    }

    /// <summary>
    /// Adds an instance data to a batch
    /// </summary>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="transform">A transform matrix</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    public void Add(Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, Matrix3x2 transform, float layerDepth = 1)
    {
        Add(new Quad(baseTexture), baseTexture, sampler, Vector2.Transform(position, transform), color, scale, Vector2.Zero, 0, layerDepth);
    }

    /// <summary>
    /// Adds an instance data to a batch
    /// </summary>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    public void Add(Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, float layerDepth = 1)
    {
        Add(new Quad(baseTexture), baseTexture, sampler, position, color, scale, Vector2.Zero, 0, layerDepth);
    }

    /// <summary>
    /// Adds an instance data to a batch
    /// </summary>
    /// <param name="quad">A spriteTexture to set a quad and coords for the texture</param>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="transform">A transform matrix</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    public void Add(Quad quad, Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, Matrix3x2 transform, float layerDepth = 1)
    {
        Add(quad, baseTexture, sampler, Vector2.Transform(position, transform), color, scale, Vector2.Zero, 0, layerDepth);
    }

    /// <summary>
    /// Adds an instance data to a batch
    /// </summary>
    /// <param name="quad">A spriteTexture to set a quad and coords for the texture</param>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    public void Add(Quad quad, Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, float layerDepth = 1)
    {
        Add(quad, baseTexture, sampler, position, color, scale, Vector2.Zero, 0, layerDepth);
    }

    /// <summary>
    /// Adds an instance data to a batch
    /// </summary>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="origin">An origin of a drawn texture</param>
    /// <param name="transform">A transform matrix</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    public void Add(Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, Vector2 origin, Matrix3x2 transform, float layerDepth = 1)
    {
        Add(new Quad(baseTexture), baseTexture, sampler, Vector2.Transform(position, transform), color, scale, origin, 0, layerDepth);
    }

    /// <summary>
    /// Adds an instance data to a batch
    /// </summary>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="origin">An origin of a drawn texture</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    public void Add(Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, Vector2 origin, float layerDepth = 1)
    {
        Add(new Quad(baseTexture), baseTexture, sampler, position, color, scale, origin, 0, layerDepth);
    }

    /// <summary>
    /// Adds an instance data to a batch
    /// </summary>
    /// <param name="quad">A spriteTexture to set a quad and coords for the texture</param>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="origin">An origin of a drawn texture</param>
    /// <param name="transform">A transform matrix</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    public void Add(Quad quad, Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, Vector2 origin, Matrix3x2 transform, float layerDepth = 1)
    {
        Add(quad, baseTexture, sampler, Vector2.Transform(position, transform), color, scale, origin, 0, layerDepth);
    }

    /// <summary>
    /// Adds an instance data to a batch
    /// </summary>
    /// <param name="quad">A spriteTexture to set a quad and coords for the texture</param>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="origin">An origin of a drawn texture</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    public void Add(Quad quad, Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, Vector2 origin, float layerDepth = 1)
    {
        Add(quad, baseTexture, sampler, position, color, scale, origin, 0, layerDepth);
    }


    public void Add(Quad quad, Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, Vector2 origin, float rotation, Matrix3x2 transform, float layerDepth = 1)
    {
        Add(quad, baseTexture, sampler, Vector2.Transform(position, transform), color, scale, origin, rotation, layerDepth);
    }

    public void Add(Quad quad, Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, Vector2 origin, float rotation, float layerDepth = 1)
    {
        if (instanceCount > 0 && (
            baseTexture.Handle != batches[batchIndex].Binding.Texture.Handle ||
            sampler.Handle != batches[batchIndex].Binding.Sampler.Handle
        ))
        {
            var pipeline = batches[batchIndex].GraphicsPipeline;
            CreateNewBatch();
            batches[batchIndex].Binding = new TextureSamplerBinding(baseTexture, sampler);
            batches[batchIndex].GraphicsPipeline = pipeline;
        }

        if (instanceCount == 0)
        {
            batches[batchIndex].Binding = new TextureSamplerBinding(baseTexture, sampler);
        }

        if (instanceCount == instancesSize)
        {
            uint maxInstances = instanceCount + 2048;
            instancesSize = maxInstances;

            transferBuffer.Dispose();
            transferBuffer = TransferBuffer.Create<InstancedVertex>(device, TransferBufferUsage.Upload, maxInstances);
        }

        transferBuffer.Map(true, out byte* mapPtr);
        InstancedVertex *instancePtr = (InstancedVertex*)mapPtr;

        instancePtr[instanceCount].Position.X = position.X;
        instancePtr[instanceCount].Position.Y = position.Y;
        instancePtr[instanceCount].Position.Z = layerDepth;
        instancePtr[instanceCount].Scale.X = quad.Width * scale.X;
        instancePtr[instanceCount].Scale.Y = quad.Height * scale.Y;
        instancePtr[instanceCount].UV0.X = quad.UV.TopLeft.X;
        instancePtr[instanceCount].UV0.Y = quad.UV.TopLeft.Y;
        instancePtr[instanceCount].UV1.X = quad.UV.BottomLeft.X;
        instancePtr[instanceCount].UV1.Y = quad.UV.BottomLeft.Y;
        instancePtr[instanceCount].UV2.X = quad.UV.TopRight.X;
        instancePtr[instanceCount].UV2.Y = quad.UV.TopRight.Y;
        instancePtr[instanceCount].UV3.X = quad.UV.BottomRight.X;
        instancePtr[instanceCount].UV3.Y = quad.UV.BottomRight.Y;
        instancePtr[instanceCount].Origin.X = origin.X;
        instancePtr[instanceCount].Origin.Y = origin.Y;
        instancePtr[instanceCount].Rotation = rotation;
        instancePtr[instanceCount].Color = color;
        transferBuffer.Unmap();

        instanceCount++;
    }

    public void BindPipeline(GraphicsPipeline pipeline)
    {
        if (instanceCount > 0 && batches[batchIndex].GraphicsPipeline.Handle != pipeline.Handle)
        {
            var binding = batches[batchIndex].Binding;
            CreateNewBatch();
            batches[batchIndex].GraphicsPipeline = pipeline;
            batches[batchIndex].Binding = binding;
            return;
        }
        if (instanceCount == 0)
        {
            batches[batchIndex].GraphicsPipeline = pipeline;
        }
    }

    private void CreateNewBatch()
    {
        CommandBuffer cmdBuf = device.AcquireCommandBuffer();
        End(cmdBuf);
        device.Submit(cmdBuf);

        batchIndex++;
        if (batchIndex >= batches.Length)
        {
            Array.Resize(ref batches, batches.Length + MaxSubBatchCount);
        }
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
            transferBuffer.Dispose();
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
