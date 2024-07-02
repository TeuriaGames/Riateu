using System;
using System.Collections.Generic;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using TeuJson;
using TeuJson.Attributes;

namespace Riateu.Graphics;

public enum Alignment 
{
    Baseline,
    Center,
    End
}

public class SpriteFont 
{
    private FontStruct fonts;
    private Dictionary<char, Character> characters = new();

    public int LineHeight;

    public SpriteFont(Texture texture, Quad quad, string jsonPath) 
    {
        fonts = JsonConvert.DeserializeFromFile<FontStruct>(jsonPath);
        LineHeight = fonts.Common.LineHeight;

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

public struct Character(int xOffset, int yOffset, int xAdvance, Quad quad)
{
    public int XOffset = xOffset;
    public int YOffset = yOffset;
    public int XAdvance = xAdvance;
    public Quad Quad = quad;
}

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