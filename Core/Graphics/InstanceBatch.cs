using System.Collections.Generic;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Components;

namespace Riateu.Graphics;

public struct SubInstanceBatch 
{
    public Buffer InstancedBuffer;
    public uint InstanceCount;
    public TextureSamplerBinding Binding;
}

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

    public Matrix4x4 Matrix;
    public bool IsDisposed { get; private set; }

    public InstanceBatch(GraphicsDevice device, int width, int height) 
    {
        Matrices = new();
        this.device = device;
        instances = new InstancedVertex[MaxInstances];

        vertexBuffer = Buffer.Create<PositionVertex>(device, BufferUsageFlags.Vertex, 4);
        indexBuffer = Buffer.Create<uint>(device, BufferUsageFlags.Index, 6);

        var model = Matrix4x4.CreateScale(1) *
            Matrix4x4.CreateRotationZ(0) *
            Matrix4x4.CreateTranslation(0, 0, 0);
        var view = Matrix4x4.CreateTranslation(0, 0, 0);
        var projection = Matrix4x4.CreateOrthographicOffCenter(0, width, 0, height, -1, 1);
        Matrix = model * view * projection;

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

    public void PushMatrix(in Matrix4x4 matrix) 
    {
        Matrices.Push(Matrix);
        Matrix = matrix;
    }

    public void PushMatrix(in Camera camera) 
    {
        PushMatrix(camera.Transform);
    }

    public void PopMatrix() 
    {
        if (Matrices.Count == 0) 
        {
            Logger.LogError("Use of PopMatrix while there is no matrix had pushed yet");
            return;
        }
        Matrix = Matrices.Pop();
    }

    public void FlushVertex(CommandBuffer cmdBuf) 
    {
        if (instanceCount == 0) 
        {
            return;
        }
        batches[batchIndex].InstanceCount = instanceCount;
        if (batches[batchIndex].InstancedBuffer != null) 
        {
            batches[batchIndex].InstancedBuffer.Dispose();
            batches[batchIndex].InstancedBuffer = Buffer.Create<InstancedVertex>(device, BufferUsageFlags.Vertex, (uint)instances.Length);
        }
        else 
        {
            batches[batchIndex].InstancedBuffer = Buffer.Create<InstancedVertex>(device, BufferUsageFlags.Vertex, (uint)instances.Length);
        }

        cmdBuf.SetBufferData(batches[batchIndex].InstancedBuffer, instances, 0, 0, instanceCount);

        instanceCount = 0;
    }

    public void Draw(CommandBuffer cmdBuf) 
    {
        Draw(cmdBuf, Matrix);
    }

    public void Draw(CommandBuffer cmdBuf, Matrix4x4 viewProjection) 
    {
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

    public void Add(
        Texture baseTexture, 
        Sampler sampler, 
        Vector2 position, 
        Matrix3x2 transform, 
        float layerDepth = 1) 
    {
        Add(new SpriteTexture(baseTexture), baseTexture, sampler, position, transform, layerDepth);
    }

    public void Add(
        SpriteTexture sTexture, 
        Texture baseTexture, 
        Sampler sampler, 
        Vector2 position, 
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
            Color.White
        );
        instanceCount++;
    }

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

    ~InstanceBatch()
    {
#if DEBUG
        Logger.LogWarn($"The type {this.GetType()} has not been disposed properly.");
#endif
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        System.GC.SuppressFinalize(this);
    }

    public void Start()
    {
        batchIndex = 0;
    }

    public void End(CommandBuffer buffer)
    {
        FlushVertex(buffer);
    }
}