using MoonWorks.Graphics;

namespace Riateu.Graphics;

public class Tileset 
{
    private Texture tilesetTexture;
    private Spritesheet spritesheet;

    public Tileset(
        Texture tileset, 
        SpriteTexture atlasTexture,
        int tileWidth,
        int tileHeight) 
    {
        tilesetTexture = tileset;
        spritesheet = new Spritesheet(tileset, atlasTexture, tileWidth, tileHeight);
    }
}