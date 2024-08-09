using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Riateu.Graphics;

public class Font : IDisposable
{
    public struct Character 
    {
        public int GlyphIndex;
        public int Width;
        public int Height;
        public float Advance;
        public float OffsetX;
        public float OffsetY;
        public float Scale;
        public bool Visible;
    }

    private IntPtr fontPtr;
    private IntPtr dataPtr;
    private bool disposedValue;
    private Dictionary<int, int> cachedCodePoints = new Dictionary<int, int>();

    public int Height => Ascent - Descent;
    public int LineHeight => Ascent - Descent + LineGap;

    public int Ascent { get; private set; }
    public int Descent { get; private set; }
    public int LineGap { get; private set; }

    public Font(string filepath) 
    {
        using var fs = File.OpenRead(filepath);
        Load(fs);
    }

    public Font(Stream stream) 
    {
        Load(stream);
    }

    private unsafe void Load(Stream stream) 
    {
        dataPtr = stream.AllocToPointer(out int length);
        fontPtr = RiateuNative.Riateu_FontInit((byte*)dataPtr);

        if (fontPtr == IntPtr.Zero) 
        {
            throw new Exception("Failed to create the font.");
        }

        RiateuNative.Riateu_FontGetMetrics(fontPtr, out int ascent, out int descent, out int lineGap);
        Ascent = ascent;
        Descent = descent;
        LineGap = lineGap;
    }

    public float GetScale(float size) 
    {
        return RiateuNative.Riateu_FontGetPixelScale(fontPtr, size);
    }

    public float GetKerning(char c1, char c2, float scale) 
    {
        return GetKerning((int)c1, (int)c2, scale);
    }

    public float GetKerning(int c1, int c2, float scale) 
    {
        int firstGlyph = FindGlyphIndex(c1);
        int secondGlyph = FindGlyphIndex(c2);
        return RiateuNative.Riateu_FontGetKerning(fontPtr, firstGlyph, secondGlyph, scale);
    }

    public Character GetCharacter(char codepoint, float scale) 
    {
        return GetCharacter((int)codepoint, scale);
    }

    public Character GetCharacter(int codepoint, float size) 
    {
        float scale = GetScale(size);
        int glyphIndex = FindGlyphIndex(codepoint);
        RiateuNative.Riateu_FontGetCharacter(fontPtr, glyphIndex, scale, 
            out int width, out int height, out float advance, out float offsetX, out float offsetY, out int visible);


        float actualOffsetY = offsetY + size;
        
        return new Character() 
        {
            GlyphIndex = glyphIndex,
            Scale = scale,
            Width = width,
            Height = height,
            Advance = advance,
            OffsetX = offsetX,
            OffsetY = actualOffsetY,
            Visible = visible == 1
        };
    }

    public int FindGlyphIndex(char codepoint) 
    {
        return FindGlyphIndex((int)codepoint);
    }

    public int FindGlyphIndex(int codepoint) 
    {
        if (cachedCodePoints.TryGetValue(codepoint, out int glyphIndex)) 
        {
            return glyphIndex;
        }
        int newGlyphIndex = RiateuNative.Riateu_FontFindGlyphIndex(fontPtr, codepoint);
        cachedCodePoints[codepoint] = newGlyphIndex;
        return newGlyphIndex;
    }

    public Image GetImage(in Character character) 
    {
        Image image = new Image(character.Width, character.Height);
        GetPixelsByCharacter(character, image.Pixels);

        return image;
    }

    public unsafe bool GetPixelsByCharacter(in Character character, Span<Color> dest) 
    {
        if (!character.Visible) 
        {
            return false;
        }

#if DEBUG
        if (dest.Length < character.Width * character.Height) 
        {
            throw new Exception($"Destination length '{dest.Length}' is lower than the Character size: '{character.Width * character.Height}'");
        }
#endif

        fixed (Color *destPtr = dest) 
        {
            RiateuNative.Riateu_FontGetPixels(fontPtr, (IntPtr)destPtr, character.GlyphIndex, character.Width, character.Height, character.Scale);
        }

        return true;
    }

    protected virtual unsafe void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            NativeMemory.Free((void*)fontPtr);
            NativeMemory.Free((void*)dataPtr);
            fontPtr = IntPtr.Zero;
            dataPtr = IntPtr.Zero;
            disposedValue = true;
        }
    }

    ~Font()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}