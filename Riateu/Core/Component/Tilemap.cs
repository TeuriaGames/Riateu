using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu.Components;


/// <summary>
/// A class that contains a collection of tiles to build a map.
/// </summary>
public class Tilemap : Component
{
    private Array2D<TextureQuad?> tiles;
    private Texture tilemapTexture;
    private Matrix4x4 Matrix;
    private int gridSize;
    /// <summary>
    /// A size of a grid in tiles.
    /// </summary>
    public int GridSize => gridSize;

    /// <summary>
    /// An initialization of a tilemap.
    /// </summary>
    /// <param name="texture">A texture used for tilemap</param>
    /// <param name="tiles">A tiles containing the map of the tiles</param>
    /// <param name="gridSize">A size of a grid in tiles</param>
    public Tilemap(Texture texture, Array2D<TextureQuad?> tiles, int gridSize)
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
    }

    /// <summary>
    /// Set a tile to a specific grid location.
    /// </summary>
    /// <param name="x">A grid x</param>
    /// <param name="y">A grid y</param>
    /// <param name="texture">A quad or null value</param>
    public void SetTile(int x, int y, TextureQuad? texture)
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

    private void AddToBatch(Batch draw)
    {
        for (int x = 0; x < tiles.Rows; x++)
        {
            for (int y = 0; y < tiles.Columns; y++)
            {
                var sTexture = tiles[x, y];
                if (sTexture is null)
                    continue;

                draw.Draw(sTexture.Value, Entity.Transform.Position + new Vector2(x * gridSize, y * gridSize), Color.White, layerDepth: 1f);
            }
        }
    }

    /// <inheritdoc/>
    public override void Draw(Batch draw)
    {
        AddToBatch(draw);
    }
}
