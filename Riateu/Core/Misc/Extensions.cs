using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using WellspringCS;

namespace Riateu;

/// <summary>
/// An extension for RiateuEngine.
/// </summary>
public static class RiateuExtensions 
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "StringBytesLength")]
    internal static extern ref int Font_StringBytesLength(Font font);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "StringBytes")]
    internal static unsafe extern ref byte* Font_StringBytes(Font font);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<Handle>k__BackingField")]
    internal static extern ref IntPtr Font_Handle(Font font);
    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    internal static extern Font Font_ctor(GraphicsDevice device, IntPtr handle, Texture texture, float pixelsPerEm, float distanceRange);

    /// <inheritdoc cref="MoonWorks.Graphics.Font.Font.TextBounds(string, int, HorizontalAlignment, VerticalAlignment, out Wellspring.Rectangle)"/>
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

    /// <summary>
    /// Loads a TTF or OTF font from a path for use in MSDF rendering.
    /// Note that there must be an msdf-atlas-gen JSON and image file alongside.
    /// </summary>
    /// <returns></returns>
    public unsafe static Font FontLoad(
        GraphicsDevice graphicsDevice,
        CommandBuffer commandBuffer,
        Stream fontFileStream,
        Stream atlasFileStream,
        Stream pngStream
    ) {
        var fontFileByteBuffer = NativeMemory.Alloc((nuint) fontFileStream.Length);
        var fontFileByteSpan = new Span<byte>(fontFileByteBuffer, (int) fontFileStream.Length);
        fontFileStream.ReadExactly(fontFileByteSpan);
        fontFileStream.Close();

        var atlasFileByteBuffer = NativeMemory.Alloc((nuint) atlasFileStream.Length);
        var atlasFileByteSpan = new Span<byte>(atlasFileByteBuffer, (int) atlasFileStream.Length);
        atlasFileStream.ReadExactly(atlasFileByteSpan);
        atlasFileStream.Close();

        var handle = Wellspring.Wellspring_CreateFont(
            (IntPtr) fontFileByteBuffer,
            (uint) fontFileByteSpan.Length,
            (IntPtr) atlasFileByteBuffer,
            (uint) atlasFileByteSpan.Length,
            out float pixelsPerEm,
            out float distanceRange
        );

        ImageUtils.ImageInfoFromStream(pngStream, out var width, out var height, out var sizeInBytes);
        var texture = Texture.CreateTexture2D(graphicsDevice, width, height, TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler);

        var transferBuffer = new TransferBuffer(graphicsDevice, sizeInBytes);
        ImageUtils.DecodeIntoTransferBuffer(
            pngStream,
            transferBuffer,
            0,
            SetDataOptions.Overwrite
        );

        commandBuffer.BeginCopyPass();
        commandBuffer.UploadToTexture(
            transferBuffer,
            texture
        );
        commandBuffer.EndCopyPass();

        transferBuffer.Dispose();


        NativeMemory.Free(fontFileByteBuffer);
        NativeMemory.Free(atlasFileByteBuffer);

        return Font_ctor(graphicsDevice, handle, texture, pixelsPerEm, distanceRange);
    }
}