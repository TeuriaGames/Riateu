using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

public struct SpriteTexture 
{
    public UV UV;

    public Rect Source;

    public int Width => Source.W;
    public int Height => Source.H;

    public SpriteTexture(Texture texture) 
        : this(
            texture, 
            new Rect(0, 0, (int)texture.Width, (int)texture.Height)

        )
    {
    }

    public SpriteTexture(Texture texture, Rect source) 
    {
        Source = source;

        var tlX = source.X / (float)texture.Width;
        var tlY = source.Y / (float)texture.Height;
        
        var rX = (source.X + source.W) / (float)texture.Width;
        var bX = (source.Y + source.H) / (float)texture.Height;

        UV = new UV(new Vector2(tlX, tlY), new Vector2(rX, bX));
    }
}

public struct UV
{
	public Vector2 Position { get; }
	public Vector2 Dimensions { get; }

	public Vector2 TopLeft { get; }
	public Vector2 TopRight { get; }
	public Vector2 BottomLeft { get; }
	public Vector2 BottomRight { get; }

	public UV(Vector2 position, Vector2 dimensions)
	{
		Position = position;
		Dimensions = dimensions;

		TopLeft = Position;
		TopRight = Position + new Vector2(Dimensions.X, 0);
		BottomLeft = Position + new Vector2(0, Dimensions.Y);
		BottomRight = Position + new Vector2(Dimensions.X, Dimensions.Y);
	}
}