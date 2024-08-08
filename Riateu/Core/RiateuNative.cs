using System;
using System.Runtime.InteropServices;

namespace Riateu;

internal static class RiateuNative 
{
    internal const string DLLName = "RiateuNative";

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe IntPtr Riateu_LoadImage(byte *data, int length, out int width, out int height, out int len);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe void Riateu_FreeImage(byte *ptr);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe int Riateu_WritePNG(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string filename,
        byte *data,
        int width,
        int height
    );

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe int Riateu_WriteQOI(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string filename,
        byte *data,
        int width,
        int height
    );

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe IntPtr Riateu_FontInit(byte *data);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe void Riateu_FontGetCharacter(
        IntPtr font, int glyph, float scale, 
        out int width, out int height, out float advance, out float offsetX, out float offsetY, 
        out int visible);
    
    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe void Riateu_FontGetPixels(IntPtr font, IntPtr data, int glyph, int width, int height, float scale);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe void Riateu_FontGetMetrics(IntPtr font, out int ascent, out int descent, out int lineGap);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe int Riateu_FontFindGlyphIndex(IntPtr font, int codepoint);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe float Riateu_FontGetPixelScale(IntPtr font, float scale);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe float Riateu_FontGetKerning(IntPtr font, int glyph1, int glyph2, float scale);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe void Riateu_FontFree(IntPtr font);
}