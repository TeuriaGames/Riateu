using System;
using System.Runtime.InteropServices;

namespace Riateu;

internal static partial class Native 
{
    internal const string DLLName = "RiateuNative";

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe IntPtr Riateu_LoadImage(byte *data, int length, out int width, out int height, out int len);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Riateu_FreeImage(IntPtr ptr);

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
    internal static extern unsafe IntPtr Riateu_LoadFont(byte *data);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Riateu_GetFontCharacter(
        IntPtr font, int glyph, float scale, 
        out int width, out int height, out float advance, out float offsetX, out float offsetY, 
        out int visible);
    
    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Riateu_GetFontPixels(IntPtr font, IntPtr data, int glyph, int width, int height, float scale);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Riateu_GetFontMetrics(IntPtr font, out int ascent, out int descent, out int lineGap);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Riateu_FindFontGlyphIndex(IntPtr font, int codepoint);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern float Riateu_GetFontPixelScale(IntPtr font, float scale);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern float Riateu_GetFontKerning(IntPtr font, int glyph1, int glyph2, float scale);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Riateu_FreeFont(IntPtr font);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe IntPtr Riateu_LoadGif(byte *data, int length);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Riateu_GetGifSize(IntPtr gif, out int width, out int height, out int len);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Riateu_GetGifFrames(IntPtr gif, out int frames);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Riateu_GetGifChannels(IntPtr gif, out int channels);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr Riateu_CopyGifFrames(IntPtr gif, int index);

    [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr Riateu_FreeGif(IntPtr gif);
}