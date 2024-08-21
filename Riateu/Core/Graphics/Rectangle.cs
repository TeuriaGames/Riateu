using System;
using System.Diagnostics.CodeAnalysis;


namespace Riateu.Graphics;

public struct Rectangle : IEquatable<Rectangle>
{
    public int X;
    public int Y;
    public int Width;
    public int Height;

    public int Left => X;
    public int Right => X + Width;
    public int Top => Y;
    public int Bottom => Y + Height;

    public Point Min => new Point(X, Y);
    public Point Max => new Point(X + Width, Y + Height);

    public Point Size => new Point(Width, Height);

    public Point Center => new Point(X + (Width / 2), Y + (Height / 2));

    public Rectangle(int x, int y, int width, int height) 
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public Rectangle(Point loc, Point size) 
    {
        X = loc.X;
        Y = loc.Y;
        Width = size.X;
        Height = size.Y;
    }

    public bool Contains(int x, int y) 
    {
        return Left <= x &&
            x < Right &&
            Top <= y &&
            y < Bottom;
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

    public Rectangle Overlap(in Rectangle other) 
    {
        bool overlapX = Right > other.Left && Left < other.Right;
        bool overlapY = Bottom > other.Top && Top < other.Bottom;

        Rectangle result = new Rectangle();

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

    public Rectangle Union(in Rectangle other) 
    {
        int x = Math.Min(this.X, other.X);
        int y = Math.Min(this.Y, other.Y);
        return new Rectangle(
            x, y,
            Math.Max(this.Right, other.Right) - x,
            Math.Max(this.Bottom, other.Bottom) - y
        );
    }

    public RectangleF ToFloat() 
    {
        return new RectangleF(X, Y, Width, Height);
    }

    public bool Equals(Rectangle other) 
    {
        return this == other;
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        return (obj is Rectangle) && this == ((Rectangle)obj);
    }

    public override int GetHashCode()
    {
        return (X ^ Y ^ Width ^ Height);
    }

    public static bool operator ==(Rectangle a, Rectangle b) 
    {
        return a.X == b.X &&
            a.Y == b.Y &&
            a.Width == b.Width &&
            a.Height == b.Height;
    }

    public static bool operator !=(Rectangle a, Rectangle b) 
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
