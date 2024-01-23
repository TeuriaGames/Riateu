using System.Collections.Generic;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Components;

namespace Riateu.Graphics;

public class InstanceBatch : System.IDisposable
{
    private static readonly float[] CornerOffsetX = [ 0.0f, 0.0f, 1.0f, 1.0f ];
    private static readonly float[] CornerOffsetY = [ 0.0f, 1.0f, 0.0f, 1.0f ];  
    private const int MaxInstances = 8192;
    private GraphicsDevice device;
    private InstancedVertex[] instances;
    private TextureSamplerBinding[] fragmentSampler;
    private uint instanceCount;

    private Buffer vertexBuffer;
    private Buffer indexBuffer;
    private Buffer instancedBuffer;
    private Stack<Matrix4x4> Matrices;

    public Matrix4x4 Matrix;

    public InstanceBatch(GraphicsDevice device, int width, int height) 
    {
        Matrices = new();
        this.device = device;
        instances = new InstancedVertex[MaxInstances];

        fragmentSampler = new TextureSamplerBinding[1];
        vertexBuffer = Buffer.Create<PositionVertex>(device, BufferUsageFlags.Vertex, 4);
        indexBuffer = Buffer.Create<uint>(device, BufferUsageFlags.Index, 6);
        instancedBuffer = Buffer.Create<InstancedVertex>(device, BufferUsageFlags.Vertex, (uint)instances.Length);

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
        Matrices.Push(matrix);
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

        cmdBuf.SetBufferData(instancedBuffer, instances, 0, 0, instanceCount);
    }

    public void Begin(TextureSamplerBinding binding) 
    {
        End();
        fragmentSampler[0] = binding;
    }

    public void End() 
    {
        instanceCount = 0;
    }

    public void Draw(CommandBuffer cmdBuf) 
    {
        Draw(cmdBuf, Matrix);
    }

    public void Draw(CommandBuffer cmdBuf, Matrix4x4 viewProjection) 
    {
        if (instanceCount == 0) 
        {
            return;
        }
        var vertexOffset = cmdBuf.PushVertexShaderUniforms(viewProjection);

        cmdBuf.BindVertexBuffers(vertexBuffer, instancedBuffer);
        cmdBuf.BindIndexBuffer(indexBuffer, IndexElementSize.Sixteen);


        cmdBuf.BindFragmentSamplers(fragmentSampler);
        cmdBuf.DrawInstancedPrimitives(0u, 0u, 2u, instanceCount, vertexOffset, 0u);
    }

    public void Add(
        SpriteTexture sTexture, Sampler sampler, Vector2 position, Matrix3x2 transform,
        FlipMode flipMode = FlipMode.None, float layerDepth = 1) 
    {
        if (instanceCount == instances.Length) 
        {
            int maxInstances = (int)(instanceCount + 2048);
            System.Array.Resize(ref instances, maxInstances);

            instancedBuffer.Dispose();
            instancedBuffer = Buffer.Create<InstancedVertex>(
                device, BufferUsageFlags.Vertex, (uint)instances.Length
            );
        }

        float width = sTexture.Source.W;
        float height = sTexture.Source.H;
        
        instances[instanceCount] = new InstancedVertex(
            new Vector3(Vector2.Transform(position, transform), layerDepth),
            new Vector2(sTexture.Width, sTexture.Height),
            sTexture.UV,
            Color.White
        );

        var flipByte = (byte)(flipMode & (FlipMode.Horizontal | FlipMode.Vertical));
        instances[instanceCount].UV0.X = CornerOffsetX[0 ^ flipByte] * sTexture.UV.Dimensions.X + sTexture.UV.Position.X;
        instances[instanceCount].UV0.Y = CornerOffsetY[0 ^ flipByte] * sTexture.UV.Dimensions.Y + sTexture.UV.Position.Y;
        instances[instanceCount].UV1.X = CornerOffsetX[1 ^ flipByte] * sTexture.UV.Dimensions.X + sTexture.UV.Position.X;
        instances[instanceCount].UV1.Y = CornerOffsetY[1 ^ flipByte] * sTexture.UV.Dimensions.Y + sTexture.UV.Position.Y;
        instances[instanceCount].UV2.X = CornerOffsetX[2 ^ flipByte] * sTexture.UV.Dimensions.X + sTexture.UV.Position.X;
        instances[instanceCount].UV2.Y = CornerOffsetY[2 ^ flipByte] * sTexture.UV.Dimensions.Y + sTexture.UV.Position.Y;
        instances[instanceCount].UV3.X = CornerOffsetX[3 ^ flipByte] * sTexture.UV.Dimensions.X + sTexture.UV.Position.X;
        instances[instanceCount].UV3.Y = CornerOffsetY[3 ^ flipByte] * sTexture.UV.Dimensions.Y + sTexture.UV.Position.Y;
        instanceCount++;
    }

    public void Dispose()
    {
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }
}