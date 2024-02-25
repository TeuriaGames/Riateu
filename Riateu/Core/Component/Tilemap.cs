using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu.Components;

/// <summary>
/// An enum containg the rendering mode for the tilemap. 
/// </summary>
public enum TilemapMode 
{
    /// <summary>
    /// Baked the texture of the tilemap.
    /// </summary>
    Baked,
    /// <summary>
    /// Cull the vertices of the tilemap to reduces draw calls.
    /// </summary>
    Cull
}

/// <summary>
/// A class that contains a collection of tiles to build a map.
/// </summary>
public class Tilemap : Component 
{
    private Array2D<Quad?> tiles;
    private Texture tilemapTexture;
    private Texture frameBuffer;
    private bool dirty = true;
    private TilemapMode mode;
    private Matrix4x4 Matrix;
    private int gridSize;
    /// <summary>
    /// A size of a grid in tiles.
    /// </summary>
    public int GridSize => gridSize;
    /// <summary> 
    /// Contains the rendering mode for the tilemap. 
    /// </summary>
    public TilemapMode Mode => mode;

    /// <summary>
    /// An initialization of a tilemap.
    /// </summary>
    /// <param name="texture">A texture used for tilemap</param>
    /// <param name="tiles">A tiles containing the map of the tiles</param>
    /// <param name="gridSize">A size of a grid in tiles</param>
    /// <param name="mode">A rendering mode for the tilemap</param>
    public Tilemap(Texture texture, Array2D<Quad?> tiles, int gridSize, 
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

        this.tiles = tiles;
        this.tilemapTexture = texture;
        this.gridSize = gridSize;

        this.mode = mode;
        if (mode == TilemapMode.Baked) 
        {
            frameBuffer = Texture.CreateTexture2D(GameContext.GraphicsDevice,
                (uint)(rows * gridSize), (uint)(columns * gridSize),
                TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler | TextureUsageFlags.ColorTarget);
        }
    }

    /// <summary>
    /// Set a tile to a specific grid location.
    /// </summary>
    /// <param name="x">A grid x</param>
    /// <param name="y">A grid y</param>
    /// <param name="texture">A quad or null value</param>
    public void SetTile(int x, int y, Quad? texture) 
    {
        if (ArrayUtils.ArrayCheck(x, y, tiles))
            tiles[x, y] = texture;
    }

    /// <summary>
    /// Clear all tiles in this map.
    /// </summary>
    public void Clear() 
    {
        tiles.Fill(null);
    }
    
    private void AddToBatch(IBatch spriteBatch) 
    {
        for (int x = 0; x < tiles.Rows; x++) 
        {
            for (int y = 0; y < tiles.Columns; y++) 
            {
                var sTexture = tiles[x, y];
                if (sTexture is null)
                    continue;
                
                spriteBatch.Add(sTexture.Value, tilemapTexture, GameContext.GlobalSampler, 
                    new Vector2(x * gridSize, y * gridSize), Color.White, Entity.Transform.WorldMatrix, layerDepth: 1f);
            }
        }
    }

    /// <inheritdoc/>
    public override void Draw(CommandBuffer buffer, IBatch spriteBatch)
    {
        var device = GameContext.GraphicsDevice;

        if (mode == TilemapMode.Baked) 
        {
            if (dirty) 
            {
                AddToBatch(spriteBatch);
                buffer.BeginRenderPass(new ColorAttachmentInfo(frameBuffer, Color.Transparent));
                buffer.BindGraphicsPipeline(GameContext.DefaultPipeline);
                spriteBatch.Draw(buffer, Matrix);
                buffer.EndRenderPass();
                dirty = false;
            }
            spriteBatch.Add(frameBuffer, GameContext.GlobalSampler, 
                Vector2.Zero, Color.White, Matrix3x2.Identity);
            return;
        }
        AddToBatch(spriteBatch);
    }

    /// <inheritdoc/>
    public override void Removed()
    {
        if (mode == TilemapMode.Baked) 
        {
            frameBuffer.Dispose();
        }
    }
}
