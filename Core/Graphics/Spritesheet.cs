using System;
using MoonWorks.Graphics;

namespace Riateu.Graphics;

public class Spritesheet 
{
    private Array2D<SpriteTexture> tiles;
    public SpriteTexture SpriteTexture { get; private set; }

    public int TileWidth => tileWidth;
    private int tileWidth;

    public int TileHeight => tileHeight;
    private int tileHeight;

    public SpriteTexture this[int x, int y] => tiles[x, y];
    public SpriteTexture this[int gid] => tiles[gid % tiles.Rows, gid / tiles.Rows];

    public Spritesheet(Texture baseTexture, SpriteTexture texture, int tileWidth, int tileHeight) 
    {
        SpriteTexture = texture;
        this.tileWidth = tileWidth;
        this.tileHeight = tileHeight;

        tiles = new Array2D<SpriteTexture>(
            SpriteTexture.Width / tileWidth, 
            SpriteTexture.Height / tileHeight);
        for (int y = 0; y < SpriteTexture.Height / tileHeight; y++) 
        {
            for (int x = 0; x < SpriteTexture.Width / tileWidth; x++) 
            {
                tiles[x, y] = new SpriteTexture(baseTexture, new Rect(
                    x * tileWidth,
                    y * tileHeight,
                    tileWidth,
                    tileHeight
                ));
            }
        }
    }

    public SpriteTexture GetTexture(int x, int y) 
    {
        return tiles[x, y];
    }

    public SpriteTexture GetTexture(int gid) 
    {
        if (gid >= 0) 
        {
            return tiles[gid % tiles.Rows, gid / tiles.Rows];
        }
        throw new ArgumentOutOfRangeException($"X: {gid % tiles.Rows} Y: {gid / tiles.Rows}");
    }
}