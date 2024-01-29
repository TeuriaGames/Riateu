using System;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

public struct SpriteTexture : IEquatable<SpriteTexture>
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

        var sx = source.X / (float)texture.Width;
        var sy = source.Y / (float)texture.Height;
        
        var sw = source.W / (float)texture.Width;
        var sh = source.H / (float)texture.Height;

        UV = new UV(new Vector2(sx, sy), new Vector2(sw, sh));
    }

    public bool Equals(SpriteTexture other)
    {
        return other.Source.X == Source.X &&
            other.Source.Y == Source.Y &&
            other.Source.W == Source.W &&
            other.Source.H == Source.H;
    }

    public void FlipUV(FlipMode flipMode) 
    {
        ReadOnlySpan<float> CornerOffsetX = [ 0.0f, 0.0f, 1.0f, 1.0f ];
        ReadOnlySpan<float> CornerOffsetY = [ 0.0f, 1.0f, 0.0f, 1.0f ];
        var flipByte = (byte)(flipMode & (FlipMode.Horizontal | FlipMode.Vertical));

        UV.TopLeft.X = CornerOffsetX[0 ^ flipByte] * UV.Dimensions.X + UV.Position.X;
        UV.TopLeft.Y = CornerOffsetY[0 ^ flipByte] * UV.Dimensions.Y + UV.Position.Y;
        UV.BottomLeft.X = CornerOffsetX[1 ^ flipByte] * UV.Dimensions.X + UV.Position.X;
        UV.BottomLeft.Y = CornerOffsetY[1 ^ flipByte] * UV.Dimensions.Y + UV.Position.Y;
        UV.TopRight.X = CornerOffsetX[2 ^ flipByte] * UV.Dimensions.X + UV.Position.X;
        UV.TopRight.Y = CornerOffsetY[2 ^ flipByte] * UV.Dimensions.Y + UV.Position.Y;
        UV.BottomRight.X = CornerOffsetX[3 ^ flipByte] * UV.Dimensions.X + UV.Position.X;
        UV.BottomRight.Y = CornerOffsetY[3 ^ flipByte] * UV.Dimensions.Y + UV.Position.Y;
    }
}

public struct UV
{
	public Vector2 Position;
	public Vector2 Dimensions;

	public Vector2 TopLeft;
	public Vector2 TopRight;
	public Vector2 BottomLeft;
	public Vector2 BottomRight;

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