using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MoonWorks;
using MoonWorks.Graphics.Font;
using WellspringCS;

namespace Riateu;

public static class RiateuExtensions 
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "StringBytesLength")]
    public static extern ref int Font_StringBytesLength(Font font);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "StringBytes")]
    public static unsafe extern ref byte* Font_StringBytes(Font font);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<Handle>k__BackingField")]
    public static unsafe extern ref IntPtr Font_Handle(Font font);

    public static unsafe bool TextBounds(
        this Font font,
        ReadOnlySpan<char> text,
        int pixelSize,
        HorizontalAlignment horizontalAlignment,
        VerticalAlignment verticalAlignment,
        out Wellspring.Rectangle rectangle
    ) {
        var byteCount = System.Text.Encoding.UTF8.GetByteCount(text);

        ref byte* stringBytes = ref Font_StringBytes(font);
        if (Font_StringBytesLength(font) < byteCount)
        {
            stringBytes = (byte*) NativeMemory.Realloc(stringBytes, (nuint) byteCount);
        }

        fixed (char* chars = text)
        {
            System.Text.Encoding.UTF8.GetBytes(chars, text.Length, stringBytes, byteCount);

            var result = Wellspring.Wellspring_TextBounds(
                Font_Handle(font),
                pixelSize,
                (Wellspring.HorizontalAlignment) horizontalAlignment,
                (Wellspring.VerticalAlignment) verticalAlignment,
                (IntPtr) stringBytes,
                (uint) byteCount,
                out rectangle
            );

            if (result == 0)
            {
                Logger.LogWarn("Could not decode string: " + new string(text));
                return false;
            }
        }

        return true;
    }
}