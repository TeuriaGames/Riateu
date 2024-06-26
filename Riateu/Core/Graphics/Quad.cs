using System;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

/// <summary>
/// A struct that defines a quad for a given texture. Can be used to 
/// define an <see cref="Riateu.Graphics.Atlas"/>.
/// </summary>
public struct Quad : IEquatable<Quad>
{
    /// <summary>
    /// A UV of a quad.
    /// </summary>
    public UV UV;

    /// <summary>
    /// A local position of a texture in screen space.
    /// </summary>
	public Vector2 Position;
    /// <summary>
    /// A local size or dimension of a texture in screen space.
    /// </summary>
	public Vector2 Dimensions;

    /// <summary>
    /// The source rect of a quad.
    /// </summary>
    public Rect Source;

    /// <summary>
    /// A width of a quad texture.
    /// </summary>
    public int Width => width;

    /// <summary>
    /// A height of a quad texture.
    /// </summary>
    public int Height => height;

    private int width;
    private int height;

    /// <summary>
    /// Initialize the <see cref="Riateu.Graphics.Quad"/> by texture.
    /// </summary>
    /// <param name="texture">A texture to based on dimensions</param>
    public Quad(Texture texture) 
        : this(
            texture, 
            new Rect(0, 0, (int)texture.Width, (int)texture.Height)

        )
    {
    }

    /// <summary>
    /// Initialize the <see cref="Riateu.Graphics.Quad"/> and specifiy its quad source.
    /// </summary>
    /// <param name="texture">A texture to based on dimensions</param>
    /// <param name="source">A quad source for the texture</param>
    public Quad(Texture texture, Rect source) 
    {
        Source = source;
        width = source.W;
        height = source.H;

        var sx = source.X / (float)texture.Width;
        var sy = source.Y / (float)texture.Height;
        
        var sw = source.W / (float)texture.Width;
        var sh = source.H / (float)texture.Height;

        UV = new UV(Position = new Vector2(sx, sy), Dimensions = new Vector2(sw, sh));
    }

    /// <summary>
    /// Initialize the <see cref="Riateu.Graphics.Quad"/> and specifiy 
    /// its quad source by the UV.
    /// </summary>
    /// <param name="texture">A texture to based on dimensions</param>
    /// <param name="uv">A UV coords of a texture to set the quad source</param>
    public Quad(Texture texture, UV uv) 
    {
        UV = uv;

        int gx = (int)(Position.X) * (int)texture.Width;
        int gy = (int)(Position.Y) * (int)texture.Height;
        
        int gw = (int)(Dimensions.X) * (int)texture.Width;
        int gh = (int)(Dimensions.Y) * (int)texture.Height;

        Source = new Rect(gx, gy, gw, gh);
    }

    /// <inheritdoc/>
    public bool Equals(Quad other)
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
        var flipByte = (byte)(flipMode & (FlipMode.Vertical | FlipMode.Horizontal));

        UV[0].X = CornerOffsetX[0 ^ flipByte] * Dimensions.X + Position.X;
        UV[0].Y = CornerOffsetY[0 ^ flipByte] * Dimensions.Y + Position.Y;
        UV[2].X = CornerOffsetX[1 ^ flipByte] * Dimensions.X + Position.X;
        UV[2].Y = CornerOffsetY[1 ^ flipByte] * Dimensions.Y + Position.Y;
        UV[1].X = CornerOffsetX[2 ^ flipByte] * Dimensions.X + Position.X;
        UV[1].Y = CornerOffsetY[2 ^ flipByte] * Dimensions.Y + Position.Y;
        UV[3].X = CornerOffsetX[3 ^ flipByte] * Dimensions.X + Position.X;
        UV[3].Y = CornerOffsetY[3 ^ flipByte] * Dimensions.Y + Position.Y;
    }
}

/// <summary>
/// A struct containing the texture coords of a texture.
/// </summary>
[System.Runtime.CompilerServices.InlineArray(4)]
public struct UV
{
    private Vector2 _element0;
    /// <summary>
    /// First index texture coords.
    /// </summary>
	public Vector2 TopLeft 
    {
        get => this[0];
        set => this[0] = value;
    }
    /// <summary>
    /// Second index texture coords.
    /// </summary>
	public Vector2 TopRight
    {
        get => this[1];
        set => this[1] = value;
    }
    /// <summary>
    /// Third index texture coords.
    /// </summary>
	public Vector2 BottomLeft
    {
        get => this[2];
        set => this[2] = value;
    }
    /// <summary>
    /// Fourth index texture coords.
    /// </summary>
	public Vector2 BottomRight
    {
        get => this[3];
        set => this[3] = value;
    }

    /// <summary>
    /// Initialize the UV of a texture.
    /// </summary>
    /// <param name="position">A local position of a texture</param>
    /// <param name="dimensions">A local size or dimension of a texture.</param>
	public UV(Vector2 position, Vector2 dimensions)
	{
		this[0] = position;
		this[1] = position + new Vector2(dimensions.X, 0);
		this[2] = position + new Vector2(0, dimensions.Y);
		this[3] = position + new Vector2(dimensions.X, dimensions.Y);
	}

    public UV(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight) 
    {
        this[0] = topLeft;
        this[1] = topRight;
        this[2] = bottomLeft;
        this[3] = bottomRight;
    }
}