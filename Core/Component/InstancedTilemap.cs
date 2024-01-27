using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu.Components;


public class InstancedTilemap : Component, System.IDisposable
{
    private Array2D<SpriteTexture?> tiles;
    private Texture tilemapTexture;
    private Texture frameBuffer;
    private TilemapMode mode;
    private Buffer vertexBuffer;
    private Buffer indexBuffer;
    private Buffer instancedBuffer;
    private InstancedVertex[] tiledData;
    private Matrix4x4 Matrix;
    public int GridSize;
    public bool IsDisposed { get; private set; }

    public InstancedTilemap(GraphicsDevice device, Texture texture, Array2D<SpriteTexture?> tiles, int gridSize, 
        TilemapMode mode = TilemapMode.Baked) 
    {
        int rows = tiles.Rows;
        int columns = tiles.Columns;
        var model = Matrix4x4.CreateScale(1) *
            Matrix4x4.CreateRotationZ(0) *
            Matrix4x4.CreateTranslation(0, 0, 0);
        var view = Matrix4x4.CreateTranslation(0, 0, 0);
        var projection = Matrix4x4.CreateOrthographicOffCenter(0, rows * gridSize, 0, columns * gridSize, -1, 1);

        Matrix = model * view * projection;

        tiledData = new InstancedVertex[rows * columns];
        this.tiles = tiles;
        this.tilemapTexture = texture;
        GridSize = gridSize;
        frameBuffer = Texture.CreateTexture2D(GameContext.GraphicsDevice,
            (uint)(rows * gridSize), (uint)(columns * gridSize),
            TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler | TextureUsageFlags.ColorTarget);
        this.mode = mode;

        vertexBuffer = Buffer.Create<PositionVertex>(
            device, BufferUsageFlags.Vertex, 4
        );

        indexBuffer = Buffer.Create<ushort>(
            device, BufferUsageFlags.Index, 6
        );

        instancedBuffer = Buffer.Create<InstancedVertex>(
            device, BufferUsageFlags.Vertex, (uint)(rows * columns)
        );
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

        RenderTiles(buffer, Vector2.Zero);

        device.Submit(buffer);
    }
    
    private unsafe uint AddToBatch(CommandBuffer buffer) 
    {
        Vector2 pos = Vector2.Zero;
        uint instances = 0;

        for (int x = 0; x < tiles.Rows; x++) 
        {
            for (int y = 0; y < tiles.Columns; y++) 
            {
                var sTexture = tiles[x, y];
                if (sTexture is null)
                    continue;
                var tex = sTexture.Value;
                fixed (InstancedVertex *ptr = &tiledData[instances]) 
                {
                    ptr->Position.X = x * GridSize;
                    ptr->Position.Y = y * GridSize;
                    ptr->Position.Z = 1;
                    ptr->UV0 = tex.UV.TopLeft;
                    ptr->UV1 = tex.UV.BottomLeft;
                    ptr->UV2 = tex.UV.TopRight;
                    ptr->UV3 = tex.UV.BottomRight;
                    ptr->Scale.X = GridSize;
                    ptr->Scale.Y = GridSize;
                    ptr->Color = Color.White;
                }
                instances++;
            }
        }

        if (instances > 0)
            buffer.SetBufferData<InstancedVertex>(instancedBuffer, tiledData, 0, 0, instances);
        return instances;
    }

    private void RenderTiles(CommandBuffer buffer, Vector2 camera) 
    {
        var instances = AddToBatch(buffer);
        buffer.BeginRenderPass(new ColorAttachmentInfo(frameBuffer, Color.Transparent));
        buffer.BindGraphicsPipeline(GameContext.InstancedPipeline);
        buffer.BindVertexBuffers(vertexBuffer, instancedBuffer);
        buffer.BindIndexBuffer(indexBuffer, IndexElementSize.Sixteen);
        buffer.BindFragmentSamplers(new TextureSamplerBinding(tilemapTexture, GameContext.GlobalSampler));
        buffer.DrawInstancedPrimitives(0, 0, 2, instances, 
            buffer.PushVertexShaderUniforms(Matrix), 0);
        buffer.EndRenderPass();
    }

    public override void Draw(CommandBuffer buffer, IBatch spriteBatch)
    {
        var device = GameContext.GraphicsDevice;

        if (mode == TilemapMode.Cull) 
        {
            RenderTiles(buffer, Vector2.Zero);
        }
        spriteBatch.Add(frameBuffer, GameContext.GlobalSampler, 
            Vector2.Zero, Matrix3x2.Identity);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                vertexBuffer.Dispose();
                indexBuffer.Dispose();
                instancedBuffer.Dispose();
                frameBuffer.Dispose();
            }

            IsDisposed = true;
        }
    }

    ~InstancedTilemap()
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
}