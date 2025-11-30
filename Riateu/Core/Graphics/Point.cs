using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Riateu.Graphics;

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct Point : IEquatable<Point>
{
    [FieldOffset(0)]
    public int X;
    [FieldOffset(4)]
    public int Y;

    public static Point Zero => default;
    public static Point UnitX => Vector128.CreateScalar(1).AsPoint();
    public static Point UnitY => Create(0, 1);
    
    public Point(int x, int y) 
    {
        this = Create(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Point Create(int x, int y) 
    {
        return Vector128.Create(x, y, 0, 0).AsPoint();
    }

    public override bool Equals(object obj) 
    {
        return (obj is Point) && Equals((Point)obj);
    }

    public bool Equals(Point other)
    {
        return AsVector128().Equals(other.AsVector128());
    }

    public override int GetHashCode()
    {
        return X ^ Y;
    }

    public static Point operator +(Point a, Point b) 
    {
        return (a.AsVector128Unsafe() + b.AsVector128Unsafe()).AsPoint();
    }

    public static Point operator -(Point a, Point b) 
    {
        return (a.AsVector128Unsafe() - b.AsVector128Unsafe()).AsPoint();
    }

    public static Point operator *(Point a, Point b) 
    {
        return (a.AsVector128Unsafe() * b.AsVector128Unsafe()).AsPoint();
    }

    public static Point operator *(Point left, int right) 
    {
        return (left.AsVector128Unsafe() * right).AsPoint();
    }

    public static Point operator *(int left, Point right) 
    {
        return right * left;
    }

    public static Point operator /(Point a, Point b) 
    {
        return (a.AsVector128Unsafe() / b.AsVector128Unsafe()).AsPoint();
    }

    public static Point operator /(Point left, int right) 
    {
        return (left.AsVector128Unsafe() / right).AsPoint();
    }

    public static bool operator ==(Point a, Point b) 
    {
        return a.AsVector128() == b.AsVector128();
    }

    public static bool operator !=(Point a, Point b) 
    {
        return a.AsVector128() != b.AsVector128();
    }

    public override string ToString()
    {
        return $"(X: {X}, Y: {Y})";
    }
    
    public Vector128<int> AsVector128() 
    {
        return Vector128.Create(X, Y, 0, 0);
    }

    public Vector128<int> AsVector128Unsafe() 
    {
        Unsafe.SkipInit(out Vector128<int> result);
        Unsafe.WriteUnaligned(ref Unsafe.As<Vector128<int>, byte>(ref result), this);
        return result;
    }
}
