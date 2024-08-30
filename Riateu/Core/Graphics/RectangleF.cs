using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;


namespace Riateu.Graphics;

public struct RectangleF : IEquatable<RectangleF>
{
    public float X;
    public float Y;
    public float Width;
    public float Height;

    public float Left => X;
    public float Right => X + Width;
    public float Top => Y;
    public float Bottom => Y + Height;

    public Vector2 Min => new Vector2(X, Y);
    public Vector2 Max => new Vector2(X + Width, Y + Height);

    public Vector2 SizeF => new Vector2(Width, Height);

    public Vector2 Center => new Vector2(X + (Width / 2), Y + (Height / 2));

    public RectangleF(float x, float y, float width, float height) 
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public RectangleF(Point loc, Point size) 
    {
        X = loc.X;
        Y = loc.Y;
        Width = size.X;
        Height = size.Y;
    }

    public bool Contains(float x, float y) 
    {
        return Left <= x &&
            x < Right &&
            Top <= y &&
            y < Bottom;
    }

    public bool Contains(int x, int y) 
    {
        return Left <= x &&
            x < Right &&
            Top <= y &&
            y < Bottom;
    }

    public bool Contains(Vector2 point) 
    {
        return Left <= point.X &&
            point.X < Right &&
            Top <= point.Y &&
            point.Y < Bottom;
    }

    public bool Contains(Point point) 
    {
        return Left <= point.X &&
            point.X < Right &&
            Top <= point.Y &&
            point.Y < Bottom;
    }

    public bool Intersects(Rectangle other) 
    {
        return other.Left < Right &&
            Left < other.Right &&
            other.Top < Bottom &&
            Top < other.Bottom;
    }

    public bool Intersects(RectangleF other) 
    {
        return other.Left < Right &&
            Left < other.Right &&
            other.Top < Bottom &&
            Top < other.Bottom;
    }

    public RectangleF Overlap(in RectangleF other) 
    {
        bool overlapX = Right > other.Left && Left < other.Right;
        bool overlapY = Bottom > other.Top && Top < other.Bottom;

        RectangleF result = new RectangleF();

        if (overlapX) 
        {
            result.X = Math.Max(Left, other.Left);
            result.Width = Math.Min(Right, other.Right) - result.X;
        }

        if (overlapY) 
        {
            result.Y = Math.Max(Top, other.Top);
            result.Height = Math.Min(Bottom, other.Bottom) - result.Y;
        }

        return result;
    }

    public RectangleF Union(in RectangleF other) 
    {
        float x = Math.Min(this.X, other.X);
        float y = Math.Min(this.Y, other.Y);
        return new RectangleF(
            x, y,
            Math.Max(this.Right, other.Right) - x,
            Math.Max(this.Bottom, other.Bottom) - y
        );
    }

    public Rectangle ToInt() 
    {
        return new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
    }


    public bool Equals(RectangleF other) 
    {
        return this == other;
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        return (obj is RectangleF) && this == ((RectangleF)obj);
    }

    public override int GetHashCode()
    {
        return ((int)X ^ (int)Y ^ (int)Width ^ (int)Height);
    }

    public static bool operator ==(RectangleF a, RectangleF b) 
    {
        return a.X == b.X &&
            a.Y == b.Y &&
            a.Width == b.Width &&
            a.Height == b.Height;
    }

    public static bool operator !=(RectangleF a, RectangleF b) 
    {
        return a.X != b.X &&
            a.Y != b.Y &&
            a.Width != b.Width &&
            a.Height != b.Height;
    }

    public override string ToString()
    {
        return $"(X: {X}, Y: {Y}, Width: {Width}, Height {Height})";
    }
}