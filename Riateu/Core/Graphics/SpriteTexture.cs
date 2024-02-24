using System;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

/// <summary>
/// A struct that defines a quad for a given texture. Can be used to 
/// define an <see cref="Riateu.Graphics.Atlas"/>.
/// </summary>
public struct SpriteTexture : IEquatable<SpriteTexture>
{
    /// <summary>
    /// A UV of a quad.
    /// </summary>
    public UV UV;

    /// <summary>
    /// The source rect of a quad.
    /// </summary>
    public Rect Source;

    /// <summary>
    /// A width of a quad texture.
    /// </summary>
    public int Width 
    {
        get => width;
        set => width = value;
    }
    /// <summary>
    /// A height of a quad texture.
    /// </summary>
    public int Height
    {
        get => height;
        set => height = value;
    }

    private int width;
    private int height;

    /// <summary>
    /// Initialize the <see cref="Riateu.Graphics.SpriteTexture"/> by texture.
    /// </summary>
    /// <param name="texture">A texture to based on dimensions</param>
    public SpriteTexture(Texture texture) 
        : this(
            texture, 
            new Rect(0, 0, (int)texture.Width, (int)texture.Height)

        )
    {
    }

    /// <summary>
    /// Initialize the <see cref="Riateu.Graphics.SpriteTexture"/> and specifiy its quad source.
    /// </summary>
    /// <param name="texture">A texture to based on dimensions</param>
    /// <param name="source">A quad source for the texture</param>
    public SpriteTexture(Texture texture, Rect source) 
    {
        Source = source;
        width = source.W;
        height = source.H;

        var sx = source.X / (float)texture.Width;
        var sy = source.Y / (float)texture.Height;
        
        var sw = source.W / (float)texture.Width;
        var sh = source.H / (float)texture.Height;

        UV = new UV(new Vector2(sx, sy), new Vector2(sw, sh));
    }

    /// <summary>
    /// Initialize the <see cref="Riateu.Graphics.SpriteTexture"/> and specifiy 
    /// its quad source by the UV.
    /// </summary>
    /// <param name="texture">A texture to based on dimensions</param>
    /// <param name="uv">A UV coords of a texture to set the quad source</param>
    public SpriteTexture(Texture texture, UV uv) 
    {
        UV = uv;

        int gx = (int)(uv.Position.X) * (int)texture.Width;
        int gy = (int)(uv.Position.Y) * (int)texture.Height;
        
        int gw = (int)(uv.Dimensions.X) * (int)texture.Width;
        int gh = (int)(uv.Dimensions.Y) * (int)texture.Height;

        Source = new Rect(gx, gy, gw, gh);
    }

    /// <inheritdoc/>
    public bool Equals(SpriteTexture other)
    {
        return other.Source.X == Source.X &&
            other.Source.Y == Source.Y &&
            other.Source.W == Source.W &&
            other.Source.H == Source.H;
    }

    /// <summary>
    /// Flip a <see cref="Riateu.Graphics.UV"/> of a quad.
    /// </summary>
    /// <param name="flipMode">An enum flag to tell where it should be flip</param>
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

/// <summary>
/// A struct containing the texture coords of a texture.
/// </summary>
public struct UV
{
    /// <summary>
    /// A local position of a texture in screen space.
    /// </summary>
	public Vector2 Position;
    /// <summary>
    /// A local size or dimension of a texture in screen space.
    /// </summary>
	public Vector2 Dimensions;

    /// <summary>
    /// First index texture coords.
    /// </summary>
	public Vector2 TopLeft;
    /// <summary>
    /// Second index texture coords.
    /// </summary>
	public Vector2 TopRight;
    /// <summary>
    /// Third index texture coords.
    /// </summary>
	public Vector2 BottomLeft;
    /// <summary>
    /// Fourth index texture coords.
    /// </summary>
	public Vector2 BottomRight;

    /// <summary>
    /// Initialize the UV of a texture.
    /// </summary>
    /// <param name="position">A local position of a texture</param>
    /// <param name="dimensions">A local size or dimension of a texture.</param>
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