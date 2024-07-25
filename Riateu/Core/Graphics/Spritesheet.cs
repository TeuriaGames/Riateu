using System;

namespace Riateu.Graphics;


/// <summary>
/// A class that aligned textures based on the given grids and creates a list of quads that
/// can be obtained by xy-grid or grid id.
/// </summary>
public class Spritesheet 
{
    private Array2D<TextureQuad> tiles;
    /// <summary>
    /// The full quad of <see cref="Riateu.Graphics.Spritesheet"/> based on the texture atlas.
    /// </summary>
    public TextureQuad SpriteTexture { get; private set; }

    /// <summary>
    /// A width of a tile grid. 
    /// </summary>
    public int TileWidth => tileWidth;
    private int tileWidth;

    /// <summary>
    /// A height of a tile grid. 
    /// </summary>
    public int TileHeight => tileHeight;
    private int tileHeight;

    /// <summary>
    /// Get a quad from xy grid.
    /// </summary>
    public TextureQuad this[int x, int y] => tiles[x, y];
    /// <summary>
    /// Get a quad from grid id.
    /// </summary>
    public TextureQuad this[int gid] => tiles[gid % tiles.Rows, gid / tiles.Rows];

    /// <summary>
    /// An initialization of this class.
    /// </summary>
    /// <param name="baseTexture">A texture to based on</param>
    /// <param name="texture">A quad to the <see cref="Riateu.Graphics.Spritesheet"/></param>
    /// <param name="tileWidth">A width of a tile grid</param>
    /// <param name="tileHeight">A height of a tile grid</param>
    public Spritesheet(Texture baseTexture, TextureQuad texture, int tileWidth, int tileHeight) 
    {
        SpriteTexture = texture;
        this.tileWidth = tileWidth;
        this.tileHeight = tileHeight;

        tiles = new Array2D<TextureQuad>(SpriteTexture.Width / tileWidth, SpriteTexture.Height / tileHeight);
        for (int y = 0; y < SpriteTexture.Height / tileHeight; y++) 
        {
            for (int x = 0; x < SpriteTexture.Width / tileWidth; x++) 
            {
                tiles[x, y] = new TextureQuad(baseTexture, new Rectangle(
                    texture.Source.X + (x * tileWidth),
                    texture.Source.Y + (y * tileHeight),
                    tileWidth,
                    tileHeight
                ));
            }
        }
    }

    /// <summary>
    /// Get a quad based on the xy-grid in Point.
    /// </summary>
    /// <param name="position">An xy-grid point position</param>
    /// <returns>A quad based on the xy-grid</returns>
    public TextureQuad GetTexture(Point position) 
    {
        return GetTexture(position.X, position.Y);
    }

    /// <summary>
    /// Get a quad based on the xy-grid in Point.
    /// </summary>
    /// <param name="x">An x-grid position</param>
    /// <param name="y">A y-grid position</param>
    /// <returns>A quad based on the xy-grid</returns>
    public TextureQuad GetTexture(int x, int y) 
    {
        return tiles[x, y];
    }

    /// <summary>
    /// Get a quad based on the grid id.
    /// </summary>
    /// <param name="gid">An id of a grid</param>
    /// <returns>A quad based on the grid id</returns>
    public TextureQuad GetTexture(int gid) 
    {
        if (gid >= 0) 
        {
            return tiles[gid % tiles.Rows, gid / tiles.Rows];
        }
        throw new ArgumentOutOfRangeException($"X: {gid % tiles.Rows} Y: {gid / tiles.Rows}");
    }
}