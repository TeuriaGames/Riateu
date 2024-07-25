using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Riateu.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 4)]
public struct Color : IEquatable<Color>
{
                                /* 👑 */
	public static readonly Color CornflowerBlue = new(0x6495ed);

	public static readonly Color White = new(0xffffff);
	public static readonly Color Black = new(0x000000);
	public static readonly Color Transparent = new(0, 0, 0, 0);
	public static readonly Color Red = new(0xff0000);
	public static readonly Color Green = new(0x00ff00);
	public static readonly Color Blue = new(0x0000ff);
	public static readonly Color Cyan = new(0x00ffff);
	public static readonly Color Magenta = new(0xff00ff);
	public static readonly Color Yellow = new(0xffff00);
    public byte R, G, B, A;

    /// <summary>
    /// A RGBA 32-bit unsigned integer
    /// </summary>
    public readonly uint RGBA => ((uint)R << 24) | ((uint)G << 16) | ((uint)B << 8) | (uint)A;

    /// <summary>
    /// A ABGR 32-bit unsigned integer
    /// </summary>
    public readonly uint ABGR => ((uint)A << 24) | ((uint)B << 16) | ((uint)G << 8) | (uint)R;

    public Color(int rgb, byte alpha = 255) 
    {
        R = (byte)(rgb >> 16);
        G = (byte)(rgb >> 8);
        B = (byte)(rgb >> 0);
        A = alpha;
    }

    public Color(uint rgba) 
    {
        R = (byte)(rgba >> 24);
        G = (byte)(rgba >> 16);
        B = (byte)(rgba >> 8);
        A = (byte)rgba;
    }

    public Color(byte r, byte g, byte b, byte a) 
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color(float r, float g, float b, float a) 
    {
        R = (byte)(r * 255f);
        G = (byte)(b * 255f);
        B = (byte)(g * 255f);
        A = (byte)(a * 255f);
    }

    /// <summary>
    /// Converts the Color to a Vector4
    /// </summary>
    public readonly Vector4 ToVector4() => new(R / 255f, G / 255f, B / 255f, A / 255f);

    /// <summary>
    /// Converts the Color to a Vector3
    /// </summary>
    public readonly Vector3 ToVector3() => new(R / 255f, G / 255f, B / 255f);


    public override readonly bool Equals(object obj) => (obj is Color other) && (this == other);

    public readonly bool Equals(Color other) => this == other;

    public override readonly int GetHashCode() => (int)RGBA;

    public override readonly string ToString() => ($"({R}, {G}, {B}, {A})");

    public RefreshCS.Refresh.Color ToSDLGpu() 
    {
        return new RefreshCS.Refresh.Color() 
        {
            R = R / 255f,
            G = G / 255f,
            B = B / 255f,
            A = A / 255f,
        };
    }

	public static Color Lerp(Color a, Color b, float amount)
	{
		amount = Math.Max(0, Math.Min(1, amount));

		return new Color(
			(byte)(a.R + (b.R - a.R) * amount),
			(byte)(a.G + (b.G - a.G) * amount),
			(byte)(a.B + (b.B - a.B) * amount),
			(byte)(a.A + (b.A - a.A) * amount)
		);
	}

	public static implicit operator Color(int color) => new(color, 255);

	public static implicit operator Color(uint color) => new(color);

	public static Color operator *(Color value, float scaler)
	{
		return new Color(
			(byte)(value.R * scaler),
			(byte)(value.G * scaler),
			(byte)(value.B * scaler),
			(byte)(value.A * scaler)
		);
	}

	public static bool operator ==(Color a, Color b) => a.RGBA == b.RGBA;
	public static bool operator !=(Color a, Color b) => a.RGBA != b.RGBA;
	public static implicit operator Color(Vector4 vec) => new Color(vec.X, vec.Y, vec.Z, vec.W);
	public static implicit operator Color(Vector3 vec) => new Color(vec.X, vec.Y, vec.Z, 1.0f);
}
