using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Riateu;

/// <summary>
/// An extension for RiateuEngine.
/// </summary>
public static class RiateuExtensions
{
    
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
}