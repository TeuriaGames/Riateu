using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Riateu.Content;
using TeuJson;
using TeuJson.Attributes;

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
/// A SpriteFont class used for rendering the text.
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
            foreach (var item in packedItems) 
            {
                Color[] color = new Color[item.Rect.Width * item.Rect.Height];
                if (Font.GetPixelsByCharacter(item.Data.Character, color)) 
                {
                    image.CopyFrom(color, item.Rect.X, item.Rect.Y, item.Rect.Width, item.Rect.Height);
                }

                availableCharacters.Add(item.Data.Index, new SpriteFontCharacter(
                    item.Data.Character.Width, item.Data.Character.Height, item.Data.Character.Advance,
                    item.Data.Character.OffsetX, item.Data.Character.OffsetY, new TextureQuad(
                        size, item.Rect
                    ),
                    item.Data.Character.Visible
                ));
            }
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

        for (int i = 0; i < text.Length; i++) 
        {
            if (text[i] == '\n') 
            {
                size.Y += LineHeight;
                if (lineWidth > size.X) 
                {
                    size.X = lineWidth;
                }
                lineWidth = 0f;
                continue;
            }

            SpriteFontCharacter c = GetCharacter(text[i]);

            lineWidth += c.Advance;
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

        for (int i = 0; i < text.Length; i++) 
        {
            if (text[i] == '\n')
                break;
            
            SpriteFontCharacter c = GetCharacter(text[i]);
            curr += c.Advance;
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
        
        var offset = Vector2.Zero;
        var lineWidth = GetLineWidth(text);
        var justified = new Vector2(lineWidth * justify.X, GetHeight(text) * justify.Y);

        for (int i = 0; i < text.Length; i++) 
        {
            if (text[i] == '\n') 
            {
                offset.X = 0;
                offset.Y += LineHeight;
                continue;
            }

            SpriteFontCharacter c = GetCharacter(text[i]);
            if (!c.Visible)
            {
                offset.X += c.Advance;
                continue;
            }

            Vector2 pos = (position + (offset + new Vector2(c.OffsetX, c.OffsetY) - justified) * scale);

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

/// <summary>
/// A character struct that holds the offset, advance and the texture coordinates.
/// </summary>
public record struct Character(int XOffset, int YOffset, int XAdvance, TextureQuad Quad);


internal partial struct FontStruct : IDeserialize 
{
    [TeuObject]
    [Name("chars")]
    public JsonCharacter[] Chars;

    [TeuObject]
    [Name("info")]
    public FontInfo Info;

    [TeuObject]
    [Name("common")]
    public FontCommon Common;
}

internal partial struct FontCommon : IDeserialize 
{
    [TeuObject]
    [Name("lineHeight")]
    public int LineHeight;
}

internal partial struct FontInfo : IDeserialize 
{
    [TeuObject]
    [Name("size")]
    public int Size;
}

internal partial struct JsonCharacter : IDeserialize
{
    [TeuObject]
    [Name("id")]
    public int ID;

    [TeuObject]
    [Name("chnl")]
    public int Channel;

    [TeuObject]
    [Name("height")]
    public int Height;

    [TeuObject]
    [Name("width")]
    public int Width;

    [TeuObject]
    [Name("page")]
    public int Page;

    [TeuObject]
    [Name("x")]
    public int X;

    [TeuObject]
    [Name("y")]
    public int Y;

    [TeuObject]
    [Name("xoffset")]
    public int XOffset;

    [TeuObject]
    [Name("yoffset")]
    public int YOffset;

    [TeuObject]
    [Name("xadvance")]
    public int XAdvance;
}