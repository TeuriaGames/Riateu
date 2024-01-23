using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu.Components;

public enum TilemapMode 
{
    Separate,
    Direct
}

public class Tilemap : Component 
{
    private Array2D<SpriteTexture?> tiles;
    private Texture tilemapTexture;
    private Texture frameBuffer;
    private bool dirty = true;
    private TilemapMode mode;
    public int GridSize;


    public Tilemap(Texture texture, Array2D<SpriteTexture?> tiles, int gridSize, 
        TilemapMode mode = TilemapMode.Separate) 
    {
        int rows = tiles.Rows;
        int columns = tiles.Columns;
        this.tiles = tiles;
        this.tilemapTexture = texture;
        GridSize = gridSize;
        frameBuffer = Texture.CreateTexture2D(GameContext.GraphicsDevice,
            1024, 640,
            TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler | TextureUsageFlags.ColorTarget);
        this.mode = mode;
    }
    
    private void AddToBatch(Batch spriteBatch, ref CommandBuffer buffer) 
    {
        for (int x = 0; x < tiles.Rows; x++) 
        {
            for (int y = 0; y < tiles.Columns; y++) 
            {
                var sTexture = tiles[x, y];
                if (sTexture is null)
                    continue;
                
                spriteBatch.Add(sTexture.Value, tilemapTexture, GameContext.GlobalSampler, 
                    new Vector2(x * GridSize, y * GridSize), Entity.Transform.WorldMatrix, layerDepth: 1f);
            }
        }

        spriteBatch.FlushVertex(buffer);
    }

    public override void Draw(Batch spriteBatch)
    {
        var device = GameContext.GraphicsDevice;
        CommandBuffer buffer = device.AcquireCommandBuffer();

        if (mode == TilemapMode.Separate) 
        {
            if (dirty) 
            {
                AddToBatch(spriteBatch, ref buffer);
                buffer.BeginRenderPass(new ColorAttachmentInfo(frameBuffer, Color.Transparent));
                buffer.BindGraphicsPipeline(GameContext.DefaultPipeline);
                spriteBatch.Draw(buffer);
                buffer.EndRenderPass();
                device.Submit(buffer);
                device.Wait();
                dirty = false;
            }
            spriteBatch.Add(frameBuffer, GameContext.GlobalSampler, 
                Vector2.Zero, Matrix3x2.Identity);
            return;
        }
        AddToBatch(spriteBatch, ref buffer);
    }
}

public class InstancedTilemap : Component 
{
    private Array2D<SpriteTexture?> tiles;
    private Texture tilemapTexture;
    private Texture frameBuffer;
    private bool dirty = true;
    private TilemapMode mode;
    private Buffer vertexBuffer;
    private Buffer indexBuffer;
    private Buffer instancedBuffer;
    private InstancedTileData[] tiledData;
    private Matrix4x4 Matrix;
    public int GridSize;


    public InstancedTilemap(GraphicsDevice device, Texture texture, Array2D<SpriteTexture?> tiles, int gridSize, 
        TilemapMode mode = TilemapMode.Separate) 
    {
        int rows = tiles.Rows;
        int columns = tiles.Columns;
        var model = Matrix4x4.CreateScale(1) *
            Matrix4x4.CreateRotationZ(0) *
            Matrix4x4.CreateTranslation(0, 0, 0);
        var view = Matrix4x4.CreateTranslation(0, 0, 0);
        var projection = Matrix4x4.CreateOrthographicOffCenter(0, rows * gridSize, 0, columns * gridSize, -1, 1);

        Matrix = model * view * projection;

        tiledData = new InstancedTileData[rows * columns];
        this.tiles = tiles;
        this.tilemapTexture = texture;
        GridSize = gridSize;
        frameBuffer = Texture.CreateTexture2D(GameContext.GraphicsDevice,
            (uint)(rows * gridSize), (uint)(columns * gridSize),
            TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler | TextureUsageFlags.ColorTarget);
        this.mode = mode;

        vertexBuffer = Buffer.Create<PositionColorVertex>(
            device, BufferUsageFlags.Vertex, 4
        );

        indexBuffer = Buffer.Create<ushort>(
            device, BufferUsageFlags.Index, 6
        );

        instancedBuffer = Buffer.Create<InstancedTileData>(
            device, BufferUsageFlags.Vertex, (uint)(rows * columns)
        );
        CommandBuffer buffer = device.AcquireCommandBuffer();

        buffer.SetBufferData<PositionColorVertex>(vertexBuffer, [
            new (new Vector3(0, 0, 0), Color.White),
            new (new Vector3(0, 1, 0), Color.White),
            new (new Vector3(1, 0, 0), Color.White),
            new (new Vector3(1, 1, 0), Color.White),
        ]);

        buffer.SetBufferData<ushort>(indexBuffer, [
            0, 1, 2, 2, 1, 3
        ]);


        device.Submit(buffer);
    }
    
    private uint AddToBatch(CommandBuffer buffer) 
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

                tiledData[instances] = new InstancedTileData(
                    new Vector3(x * GridSize, y * GridSize, 1),
                    GridSize,
                    tex.UV);
                instances++;
            }
        }

        if (instances > 0)
            buffer.SetBufferData<InstancedTileData>(instancedBuffer, tiledData, 0, 0, instances);
        return instances;
    }

    public override void Draw(Batch spriteBatch)
    {
        var device = GameContext.GraphicsDevice;
        CommandBuffer buffer = device.AcquireCommandBuffer();

        if (dirty) 
        {
            var instances = AddToBatch(buffer);
            buffer.BeginRenderPass(new ColorAttachmentInfo(frameBuffer, Color.Transparent));
            buffer.BindGraphicsPipeline(GameContext.TilemapPipeline);
            buffer.BindVertexBuffers(vertexBuffer, instancedBuffer);
            buffer.BindIndexBuffer(indexBuffer, IndexElementSize.Sixteen);
            buffer.BindFragmentSamplers(new TextureSamplerBinding(tilemapTexture, GameContext.GlobalSampler));
            buffer.DrawInstancedPrimitives(0, 0, 2, instances, 
                buffer.PushVertexShaderUniforms(Matrix), 0);
            buffer.EndRenderPass();
            device.Submit(buffer);
            // dirty = false;
        }
        spriteBatch.Add(frameBuffer, GameContext.GlobalSampler, 
            Vector2.Zero, Matrix3x2.Identity);
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct InstancedTileData(Vector3 position, float scale, UV uv) : IVertexType
{
    public Vector3 Position = position;
    public Vector2 UV0 = uv.TopLeft;
    public Vector2 UV1 = uv.BottomLeft;
    public Vector2 UV2 = uv.TopRight;
    public Vector2 UV3 = uv.BottomRight;
    public Vector2 Scale = new Vector2(scale);

    public static VertexElementFormat[] Formats => [
        VertexElementFormat.Vector3,

        VertexElementFormat.Vector2,
        VertexElementFormat.Vector2,
        VertexElementFormat.Vector2,
        VertexElementFormat.Vector2,

        VertexElementFormat.Vector2,
    ];
}