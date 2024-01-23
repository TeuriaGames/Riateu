using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu;

public abstract class Text 
{
    public Texture Texture { get; protected set; }
    protected int pixelSize;
    protected string text;
    public Rectangle Bounds { get; protected set; }

    public abstract void Draw(Batch batch, Vector2 position);
}
