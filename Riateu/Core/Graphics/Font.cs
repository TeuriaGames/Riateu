using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace Riateu.Graphics;

/// <summary>
/// A Font class loaded from a CPU meant for loading .ttf files. Used for creating a <see cref="Riateu.Graphics.SpriteFont"/>.
/// </summary>
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

    /// <summary>
    /// Load a .ttf font from a file path.
    /// </summary>
    /// <param name="filepath">A file to the .ttf font file</param>
    public Font(string filepath) 
    {
        using var fs = File.OpenRead(filepath);
        Load(fs);
    }

    /// <summary>
    /// Load a .ttf font from a stream.
    /// </summary>
    /// <param name="stream">A stream containing the .ttf font file</param>
    public Font(Stream stream) 
    {
        Load(stream);
    }

    private unsafe void Load(Stream stream) 
    {
        dataPtr = stream.AllocToPointer(out int length, out Span<byte> b);
        fixed (byte *ptr = b) 
        {
            fontPtr = Native.Riateu_LoadFont(ptr);
        }

        if (fontPtr == IntPtr.Zero) 
        {
            throw new Exception("Failed to create the font.");
        }

        Native.Riateu_GetFontMetrics(fontPtr, out int ascent, out int descent, out int lineGap);
        Ascent = ascent;
        Descent = descent;
        LineGap = lineGap;
    }

    /// <summary>
    /// Get a total scaling from a size.
    /// </summary>
    /// <param name="size">A size to convert into a scale</param>
    /// <returns>A scale from a size</returns>
    public float GetScale(float size) 
    {
        return Native.Riateu_GetFontPixelScale(fontPtr, size);
    }

    /// <summary>
    /// Get kerning between the two characters.
    /// </summary>
    /// <param name="c1">The current character</param>
    /// <param name="c2">The next character</param>
    /// <param name="scale">The scale of these characters</param>
    /// <returns>A kerning value between these two characters</returns>
    public float GetKerning(char c1, char c2, float scale) 
    {
        return GetKerning((int)c1, (int)c2, scale);
    }

    /// <summary>
    /// Get kerning between the two characters.
    /// </summary>
    /// <param name="c1">The current character index</param>
    /// <param name="c2">The next character index</param>
    /// <param name="scale">The scale of these characters</param>
    /// <returns>A kerning value between these two characters</returns>
    public float GetKerning(int c1, int c2, float scale) 
    {
        int firstGlyph = FindGlyphIndex(c1);
        int secondGlyph = FindGlyphIndex(c2);
        return Native.Riateu_GetFontKerning(fontPtr, firstGlyph, secondGlyph, scale);
    }

    /// <summary>
    /// Get a <see cref="Riateu.Graphics.Font.Character"/> from a font with codepoint.
    /// </summary>
    /// <param name="codepoint">A character represents a font character</param>
    /// <param name="size">A size of the character</param>
    /// <returns>A <see cref="Riateu.Graphics.Font.Character"/></returns>
    public Character GetCharacter(char codepoint, float size) 
    {
        return GetCharacter((int)codepoint, size);
    }

    /// <summary>
    /// Get a <see cref="Riateu.Graphics.Font.Character"/> from a font with codepoint.
    /// </summary>
    /// <param name="codepoint">A character index from a char value</param>
    /// <param name="size">A size of the character</param>
    /// <returns>A <see cref="Riateu.Graphics.Font.Character"/></returns>
    public Character GetCharacter(int codepoint, float size) 
    {
        float scale = GetScale(size);
        int glyphIndex = FindGlyphIndex(codepoint);
        Native.Riateu_GetFontCharacter(fontPtr, glyphIndex, scale, 
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

    /// <summary>
    /// Find a character glyph from a character.
    /// </summary>
    /// <param name="codepoint">A character</param>
    /// <returns>A glyph index from this character</returns>
    public int FindGlyphIndex(char codepoint) 
    {
        return FindGlyphIndex((int)codepoint);
    }

    /// <summary>
    /// Find a character glyph from a character.
    /// </summary>
    /// <param name="codepoint">A character index</param>
    /// <returns>A glyph index from this character</returns>
    public int FindGlyphIndex(int codepoint) 
    {
        if (cachedCodePoints.TryGetValue(codepoint, out int glyphIndex)) 
        {
            return glyphIndex;
        }
        int newGlyphIndex = Native.Riateu_FindFontGlyphIndex(fontPtr, codepoint);
        cachedCodePoints[codepoint] = newGlyphIndex;
        return newGlyphIndex;
    }

    /// <summary>
    /// Generate an <see cref="Riateu.Graphics.Image"/> from a character.
    /// </summary>
    /// <param name="character">
    /// A character to generate an <see cref="Riateu.Graphics.Image"/>. Use <see cref="Riateu.Graphics.Font.GetCharacter(char, float)"/>
    /// to get a character.
    /// </param>
    /// <returns>A generated character <see cref="Riateu.Graphics.Image"/> from a font bitmap</returns>
    public Image GetImageByCharacter(in Character character) 
    {
        Image image = new Image(character.Width, character.Height);
        GetPixelsByCharacter(character, image.Pixels);

        return image;
    }

    /// <summary>
    /// Generate a pixels from a <see cref="Riateu.Graphics.Font.Character"/>.
    /// </summary>
    /// <param name="character">A <see cref="Riateu.Graphics.Font.Character"/> to generate with</param>
    /// <param name="dest">The destination of a pixels to written with. It must be an array of <see cref="Riateu.Graphics.Color"/></param>
    /// <returns>Whether it suceeed to generate the pixels (true) or it failed (false)</returns>
    public virtual unsafe bool GetPixelsByCharacter(in Character character, Span<Color> dest) 
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
            Native.Riateu_GetFontPixels(fontPtr, (IntPtr)destPtr, character.GlyphIndex, character.Width, character.Height, character.Scale);
        }

        return true;
    }

    /// <summary>
    /// Dispose all of the <see cref="Riateu.Graphics.Font"/> resources.
    /// </summary>
    /// <param name="disposing">Whether to dispose the managed resources</param>
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

    ///
    ~Font()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    /// <summary>
    /// Dispose all of the <see cref="Riateu.Graphics.Font"/> resources.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

[Experimental("RIE002")]
public class MSDFFont : Font
{
    public MSDFFont(string filepath) : base(filepath)
    {
    }

    public MSDFFont(Stream stream) : base(stream)
    {
    }

    public override bool GetPixelsByCharacter(in Character character, Span<Color> dest)
    {
        // TODO generate MSDF font instead
        return base.GetPixelsByCharacter(character, dest);
    }
}