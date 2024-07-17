using System;
using System.Collections.Generic;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
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
    private FontStruct fonts;
    private Dictionary<char, Character> characters = new();
    private int lineHeight;

    /// <summary>
    /// A line height of the font.
    /// </summary>
    public int LineHeight 
    {
        get => lineHeight;
        set => lineHeight = value;
    }

    /// <summary>
    /// Creates a spritefont
    /// </summary>
    /// <param name="texture">A texture to be used</param>
    /// <param name="quad">A texture cooordinates from the texture</param>
    /// <param name="jsonPath">A path to the font data [fontbm]</param>
    public SpriteFont(Texture texture, Quad quad, string jsonPath) 
    {
        fonts = JsonConvert.DeserializeFromFile<FontStruct>(jsonPath);
        lineHeight = fonts.Common.LineHeight;

        foreach (var character in fonts.Chars) 
        {
            char c = (char)character.ID;
            Quad newQuad = new Quad(texture, new Rect(
                quad.Source.X + character.X,
                quad.Source.Y + character.Y,
                character.Width,
                character.Height
            ));

            Character ch = new Character(character.XOffset, character.YOffset, character.XAdvance, newQuad);

            characters.Add(c, ch);
        }
    }

    /// <summary>
    /// Creates a spritefont
    /// </summary>
    /// <param name="texture">A texture to be used</param>
    /// <param name="jsonPath">A path to the font data [fontbm]</param>
    public SpriteFont(Texture texture, string jsonPath) 
    {
        fonts = JsonConvert.DeserializeFromFile<FontStruct>(jsonPath);
        lineHeight = fonts.Common.LineHeight;

        foreach (var character in fonts.Chars) 
        {
            char c = (char)character.ID;
            Quad newQuad = new Quad(texture);

            Character ch = new Character(character.XOffset, character.YOffset, character.XAdvance, newQuad);

            characters.Add(c, ch);
        }
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

        Vector2 size = new Vector2(0, lineHeight);
        float lineWidth = 0f;

        for (int i = 0; i < text.Length; i++) 
        {
            if (text[i] == '\n') 
            {
                size.Y += lineHeight;
                if (lineWidth > size.X) 
                {
                    size.X = lineWidth;
                }
                lineWidth = 0f;
                continue;
            }

            if (characters.TryGetValue(text[i], out Character c)) 
            {
                lineWidth += c.XAdvance;
            }
#if DEBUG
            else 
            {
                throw new Exception($"This character '{text[i]}' or id: {(int)text[i]} does not exists.");
            }
#endif
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
            
            if (characters.TryGetValue(text[i], out Character c)) 
            {
                curr += c.XAdvance;
            }
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

        return lines * lineHeight;
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
                offset.Y += lineHeight;
                continue;
            }

            if (characters.TryGetValue(text[i], out Character c)) 
            {
                Vector2 pos = (position + (offset + new Vector2(c.XOffset, c.YOffset) - justified) * scale);

                computeData[vertexIndex] = new Batch.ComputeData 
                {
                    Position = pos,
                    Scale = scale,
                    Origin = Vector2.Zero,
                    UV = new UV(c.Quad.UV[0], c.Quad.UV[1], c.Quad.UV[2], c.Quad.UV[3]),
                    Dimension = new Vector2(c.Quad.Source.W, c.Quad.Source.H),
                    Rotation = 0,
                    Color = color.ToVector4(),
                };

                offset.X += c.XAdvance;

                vertexIndex++;
            }
        }
    }
}

/// <summary>
/// A character struct that holds the offset, advance and the texture coordinates.
/// </summary>
public record struct Character(int XOffset, int YOffset, int XAdvance, Quad Quad);


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