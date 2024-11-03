using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using Riateu.Graphics;

namespace Riateu;

/// <summary>
/// An extension for RiateuEngine.
/// </summary>
public static class RiateuExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point AsPoint(this Vector128<int> vector) 
    {
        ref byte address = ref Unsafe.As<Vector128<int>, byte>(ref vector);
        return Unsafe.ReadUnaligned<Point>(ref address);        
    }
}

public static class StreamUtils 
{
    public static unsafe IntPtr AllocToPointer(this Stream stream, out int length) 
    {
        IntPtr ptr = (IntPtr)NativeMemory.Alloc((nuint)stream.Length);
        Span<byte> fileDataSpan = new Span<byte>((void*)ptr, (int)stream.Length);
        stream.ReadExactly(fileDataSpan);
        length = fileDataSpan.Length;
        return ptr;
    }

    public static unsafe IntPtr AllocToPointer(this Stream stream, out int length, scoped out Span<byte> span) 
    {
        IntPtr ptr = (IntPtr)NativeMemory.Alloc((nuint)stream.Length);
        Span<byte> fileDataSpan = new Span<byte>((void*)ptr, (int)stream.Length);
        stream.ReadExactly(fileDataSpan);
        length = fileDataSpan.Length;
        span = fileDataSpan;
        return ptr;
    }
}