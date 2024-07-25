using System;


namespace Riateu.Graphics;

public struct Point(int x, int y) : IEquatable<Point>
{
    public int X = x;
    public int Y = y;

    public override bool Equals(object obj) 
    {
        return (obj is Point) && Equals((Point)obj);
    }

    public bool Equals(Point other)
    {
        return this == other;
    }

    public override int GetHashCode()
    {
        return X ^ Y;
    }

    public static Point operator +(Point a, Point b) 
    {
        return new Point(a.X - b.X, a.Y - b.Y);
    }

    public static Point operator -(Point a, Point b) 
    {
        return new Point(a.X - b.X, a.Y - b.Y);
    }

    public static Point operator *(Point a, Point b) 
    {
        return new Point(a.X * b.X, a.Y * b.Y);
    }

    public static Point operator /(Point a, Point b) 
    {
        return new Point(a.X / b.X, a.Y / b.Y);
    }

    public static bool operator ==(Point a, Point b) 
    {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(Point a, Point b) 
    {
        return a.X != b.X && a.Y != b.Y;
    }

    public override string ToString()
    {
        return $"(X: {X}, Y: {Y})";
    }
}
