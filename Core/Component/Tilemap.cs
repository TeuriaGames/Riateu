using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu.Components;

public enum TilemapMode 
{
    Baked,
    Cull
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
        TilemapMode mode = TilemapMode.Baked) 
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

    public override void Draw(CommandBuffer buffer, Batch spriteBatch)
    {
        var device = GameContext.GraphicsDevice;

        if (mode == TilemapMode.Baked) 
        {
            if (dirty) 
            {
                AddToBatch(spriteBatch, ref buffer);
                buffer.BeginRenderPass(new ColorAttachmentInfo(frameBuffer, Color.Transparent));
                buffer.BindGraphicsPipeline(GameContext.DefaultPipeline);
                spriteBatch.Draw(buffer);
                buffer.EndRenderPass();
                dirty = false;
            }
            spriteBatch.Add(frameBuffer, GameContext.GlobalSampler, 
                Vector2.Zero, Matrix3x2.Identity);
            return;
        }
        AddToBatch(spriteBatch, ref buffer);
    }
}
