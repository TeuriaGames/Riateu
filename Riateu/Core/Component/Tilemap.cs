using System;
using System.Numerics;
using Riateu.Graphics;

namespace Riateu.Components;


/// <summary>
/// A class that contains a collection of tiles to build a map.
/// </summary>
public class Tilemap : Component
{
    private int rows;
    private int columns;
    private Array2D<TextureQuad?> tiles;
    private int gridSize;
    private Camera cullingCamera;
    /// <summary>
    /// A size of a grid in tiles.
    /// </summary>
    public int GridSize => gridSize;

    public int Rows => rows;
    public int Columns => columns;


    /// <summary>
    /// An initialization of a tilemap.
    /// </summary>
    /// <param name="tiles">A tiles containing the map of the tiles</param>
    /// <param name="gridSize">A size of a grid in tiles</param>
    /// <param name="cullingCamera">A camera for culling</param>
    public Tilemap(Array2D<TextureQuad?> tiles, int gridSize, Camera cullingCamera)
    {
        rows = tiles.Rows;
        columns = tiles.Columns;

        this.tiles = tiles;
        this.gridSize = gridSize;
        this.cullingCamera = cullingCamera;
    }

    /// <summary>
    /// An initialization of a tilemap.
    /// </summary>
    /// <param name="tiles">A tiles containing the map of the tiles</param>
    /// <param name="gridSize">A size of a grid in tiles</param>
    public Tilemap(Array2D<TextureQuad?> tiles, int gridSize) : this(tiles, gridSize, null) {}

    public void ChangeTiles(Array2D<TextureQuad?> tiles) 
    {
        rows = tiles.Rows;
        columns = tiles.Columns;
        this.tiles = tiles;
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

    public void SetCullingCamera(Camera camera) 
    {
        this.cullingCamera = camera;
    }

    public Rectangle GetCulledRectangle() 
    {
        int x = 0, y = 0, w = rows, h = columns;

        if (cullingCamera != null) 
        {
            x = (int)Math.Max(0f, Math.Floor((-cullingCamera.Position.X - Entity.PosX) / gridSize));
            y = (int)Math.Max(0f, Math.Floor((-cullingCamera.Position.Y - Entity.PosY) / gridSize));
            w = (int)Math.Min(rows,    Math.Ceiling((-cullingCamera.Position.X + cullingCamera.Viewport.Width - Entity.PosX) / gridSize));
            h = (int)Math.Min(columns, Math.Ceiling((-cullingCamera.Position.Y + cullingCamera.Viewport.Height - Entity.PosY) / gridSize));
        }

        x = Math.Max(x, 0);
        y = Math.Max(y, 0);
        w = Math.Min(w, rows);
        h = Math.Min(h, columns);

        return new Rectangle(x, y, w + x, h + y);
    }

    /// <summary>
    /// Clear all tiles in this map.
    /// </summary>
    public void Clear()
    {
        tiles.Fill(null);
    }

    /// <inheritdoc/>
    public override void Draw(Batch draw)
    {
        Rectangle culledRect = GetCulledRectangle();
        for (int x = culledRect.X; x < culledRect.Width; x++)
        {
            for (int y = culledRect.Y; y < culledRect.Height; y++)
            {
                if (x >= rows || y >= columns)
                {
                    continue;
                }
                var sTexture = tiles[x, y];
                if (sTexture is null)
                    continue;

                draw.Draw(sTexture.Value, Entity.Position + new Vector2(x * gridSize, y * gridSize), Color.White, Entity.LayerDepth);
            }
        }
    }
}
