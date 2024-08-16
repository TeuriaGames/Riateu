using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Riateu.Content;

namespace Riateu.Graphics;

/// <summary>
/// A font alignment.
/// </summary>
public enum FontAlignment 
{
    /// <summary>
    /// An alignment from the start.
    /// </summary>
    Baseline,
    /// <summary>
    /// An alignment measured from the center. 
    /// </summary>
    Center,
    /// <summary>
    /// An alignment measure from the end.
    /// </summary>
    End
}

/// <summary>
/// A SpriteFont class used for rendering the text. Unlike <see cref="Riateu.Graphics.Font"/> this can be sent down to the GPU.
/// Which is actually intended to be used for rendering.
/// </summary>
public class SpriteFont : IAssets
{
    public record struct SpriteFontCharacter(
        int Width,
        int Height,
        float Advance,
        float OffsetX,
        float OffsetY,
        TextureQuad Quad,
        bool Visible
    );
    private record struct FontItem(int Index, Font.Character Character);

    /// <summary>
    /// A line height of the font.
    /// </summary>
    public int LineHeight => Ascent - Descent + LineGap;
    public float Height => Ascent - Descent;

    public readonly Font Font;
    public readonly float Size;

    public int Ascent;
    public int Descent;
    public int LineGap;
    public Texture Texture => fontTexture;

    private float fontScale;
    private Texture fontTexture;

    private Dictionary<int, SpriteFontCharacter> availableCharacters = new Dictionary<int, SpriteFontCharacter>();

    public static ReadOnlySpan<int> DefaultCharset => DefineCharset(0x20, 0x7f);

    public SpriteFont(ResourceUploader uploader, string path, float size, ReadOnlySpan<int> charset) 
        :this(uploader, new Font(path), size, charset)
    {

    }

    public SpriteFont(ResourceUploader uploader, string path, float size) 
        :this(uploader, new Font(path), size)
    {

    }

    public SpriteFont(ResourceUploader uploader, Font font, float size) 
        :this(uploader, font, size, DefaultCharset)
    {

    }

    public SpriteFont(ResourceUploader uploader, Font font, float size, ReadOnlySpan<int> charset) 
    {
        Font = font;
        Size = size;
        fontScale = font.GetScale(size);
        Ascent = font.Ascent;
        Descent = font.Descent;
        LineGap = font.LineGap;

        fontTexture = CreateTexture(uploader, charset);
    }

    public static ReadOnlySpan<int> DefineCharset(int start, int end) 
    {
        Span<int> span = new int[end - start];
        for (int i = 0, j = start; i < span.Length; i++, j++) 
        {
            span[i] = j;
        }

        return span;
    }

    private unsafe Texture CreateTexture(ResourceUploader uploader, ReadOnlySpan<int> charset) 
    {
        Packer<FontItem> packer = new Packer<FontItem>(8192);
        for (int i = 0; i < charset.Length; i++) 
        {
            Font.Character character = Font.GetCharacter(charset[i], Size);
            packer.Add(
                new Packer<FontItem>.Item(
                    new FontItem(charset[i], character),
                    character.Width, character.Height
                )
            );
        }

        if (packer.Pack(out List<Packer<FontItem>.PackedItem> packedItems, out Point size)) 
        {
            Image image = new Image(size.X, size.Y);
            ConcurrentDictionary<int, SpriteFontCharacter> concurrentAvailableCharacters = new ConcurrentDictionary<int, SpriteFontCharacter>();

            new Workload((i, id) => {
                Packer<FontItem>.PackedItem item = packedItems[i]; 
                Color[] color = new Color[item.Rect.Width * item.Rect.Height];
                if (Font.GetPixelsByCharacter(item.Data.Character, color)) 
                {
                    image.CopyFrom(color, item.Rect.X, item.Rect.Y, item.Rect.Width, item.Rect.Height);
                }

                concurrentAvailableCharacters.GetOrAdd(item.Data.Index, new SpriteFontCharacter(
                    item.Data.Character.Width, item.Data.Character.Height, item.Data.Character.Advance,
                    item.Data.Character.OffsetX, item.Data.Character.OffsetY, new TextureQuad(
                        size, item.Rect
                    ),
                    item.Data.Character.Visible
                ));
                return true;
            }, packedItems.Count).Finish(4);

            // ConcurrentDictionary is pretty slow that we had to fallback to Dictionary
            availableCharacters = concurrentAvailableCharacters.ToDictionary<int, SpriteFontCharacter>();

            return uploader.CreateTexture2D<Color>(image.Pixels, (uint)image.Width, (uint)image.Height);
        }

        return null;
    }

    /// <summary>
    /// Measure a width and height based on the string.
    /// </summary>
    /// <param name="text">A string to measure on</param>
    /// <returns>A measured width and height</returns>
    public Vector2 Measure(ReadOnlySpan<char> text) 
    {
        if (text.IsEmpty)
            return Vector2.Zero;

        Vector2 size = new Vector2(0, LineHeight);
        float lineWidth = 0f;
        int lastCodePoint = 0;

        for (int i = 0; i < text.Length; i++) 
        {
            char ch = text[i];
            if (ch == '\n') 
            {
                size.Y += LineHeight;
                if (lineWidth > size.X) 
                {
                    size.X = lineWidth;
                }
                lineWidth = 0f;
                continue;
            }

            SpriteFontCharacter c = GetCharacter(ch);

            lineWidth += c.Advance;
            if (lastCodePoint != 0) 
            {
                lineWidth += Font.GetKerning(lastCodePoint, ch, fontScale);
            }
            lastCodePoint = ch;
        }

        if (lineWidth > size.X) 
        {
            size.X = lineWidth;
        }

        return size;
    }

    /// <summary>
    /// Measure a line width based on the string.
    /// </summary>
    /// <param name="text">A string to measure on</param>
    /// <returns>A measured line width</returns>
    public float GetLineWidth(ReadOnlySpan<char> text) 
    {
        float curr = 0f;
        int lastCodePoint = 0;

        for (int i = 0; i < text.Length; i++) 
        {
            char ch = text[i];
            if (ch == '\n')
                break;
            
            SpriteFontCharacter c = GetCharacter(ch);
            curr += c.Advance;
            if (lastCodePoint != 0) 
            {
                curr += Font.GetKerning(lastCodePoint, ch, fontScale);
            }
            lastCodePoint = ch;
        }

        return curr;
    }

    /// <summary>
    /// Measure a height based on the string.
    /// </summary>
    /// <param name="text">A string to measure on</param>
    /// <returns>A measured height</returns>
    public float GetHeight(ReadOnlySpan<char> text) 
    {
        int lines = 1;
        if (text.IndexOf('\n') >= 0) 
        {
            for (int i = 0; i < text.Length; i++) 
            {
                if (text[i] == '\n') 
                {
                    lines++;
                }
            }
        }

        return lines * LineHeight;
    }

    public SpriteFontCharacter GetCharacter(char c) 
    {
        return GetCharacter((int)c);
    }

    public SpriteFontCharacter GetCharacter(int c) 
    {
        if (availableCharacters.TryGetValue(c, out SpriteFontCharacter ch)) 
        {
            return ch;
        }

        throw new Exception($"A character: '{(char)c}' is not available");
    }

    internal unsafe void Draw(Batch.ComputeData* computeData, ref uint vertexIndex, ReadOnlySpan<char> text, Vector2 position, Vector2 justify, Color color, Vector2 scale)
    {
        if (text.IsEmpty)
            return;
        
        var lastCodePoint = 0;
        var offset = Vector2.Zero;
        var lineWidth = GetLineWidth(text);
        var justified = new Vector2(lineWidth * justify.X, GetHeight(text) * justify.Y);

        for (int i = 0; i < text.Length; i++) 
        {
            char ch = text[i];
            if (ch == '\n') 
            {
                offset.X = 0;
                offset.Y += LineHeight;
                continue;
            }

            SpriteFontCharacter c = GetCharacter(ch);
            if (!c.Visible)
            {
                offset.X += c.Advance;
                continue;
            }

            Vector2 pos = (position + (offset + new Vector2(c.OffsetX, c.OffsetY) - justified) * scale);
            if (lastCodePoint != 0) 
            {
                pos.X += Font.GetKerning(lastCodePoint, ch, fontScale);
            }

            lastCodePoint = ch;

            computeData[vertexIndex] = new Batch.ComputeData 
            {
                Position = pos,
                Scale = scale,
                Origin = Vector2.Zero,
                UV = new UV(c.Quad.UV[0], c.Quad.UV[1], c.Quad.UV[2], c.Quad.UV[3]),
                Dimension = new Vector2(c.Quad.Source.Width, c.Quad.Source.Height),
                Rotation = 0,
                Color = color.ToVector4(),
            };

            offset.X += c.Advance;

            vertexIndex++;
        }
    }
}
