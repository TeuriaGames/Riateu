using MoonWorks.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using TeuJson;
using TeuJson.Attributes;

namespace Riateu.Graphics;

public class Tileset 
{
    private Texture tilesetTexture;
    private Spritesheet spritesheet;
    private Dictionary<byte, Ruleset> rules = new();

    private Tileset(
        Texture tileset, 
        SpriteTexture atlasTexture,
        int tileWidth,
        int tileHeight) 
    {
        tilesetTexture = tileset;
        spritesheet = new Spritesheet(tileset, atlasTexture, tileWidth, tileHeight);
    }

    public static Tileset Create(string rulesPath, Texture texture, Atlas atlas) 
    {
        using var fs = File.OpenRead(rulesPath);
        return Create(fs, texture, atlas);
    }

    public static Tileset Create(Stream rulesStream, Texture texture, Atlas atlas) 
    {
        var json = JsonTextReader.FromStream(rulesStream);
        int width = json["width"];
        int height = json["height"];
        var texCoord = atlas[json["path"]];
        Tileset tileset = new Tileset(texture, texCoord, width, height);

        var jsonRulesets = json["rules"].ConvertToArray<JsonRuleset>();

        var rulesets = new Ruleset[jsonRulesets.Length];

        for (int i = 0; i < jsonRulesets.Length; i++) 
        {
            var jsonRule = jsonRulesets[i];
            if (jsonRule.Mask == null) { continue; }
            var ruleset = new Ruleset(jsonRule.Name, jsonRule.Tiles.GetLength(0));

            for (int j = 0; j < jsonRule.Tiles.GetLength(0); j++) 
            {
                var x = jsonRule.Tiles[j, 0] - 1;
                var y = jsonRule.Tiles[j, 1] - 1;
                ruleset.Tiles[j] = tileset.spritesheet.GetTexture(x, y);
            }

            byte bit = 0;

            bit += (byte)(jsonRule.Mask[0] * 1 << 0);
            bit += (byte)(jsonRule.Mask[1] * 1 << 1);
            bit += (byte)(jsonRule.Mask[2] * 1 << 2);
            bit += (byte)(jsonRule.Mask[3] * 1 << 3);
            bit += (byte)(jsonRule.Mask[5] * 1 << 4);
            bit += (byte)(jsonRule.Mask[6] * 1 << 5);
            bit += (byte)(jsonRule.Mask[7] * 1 << 6);
            bit += (byte)(jsonRule.Mask[8] * 1 << 7);
            
            tileset.rules[bit] = ruleset;
        }
        return tileset;
    }

    public SpriteTexture[] GetTilesFromBit(byte bit) 
    {
        if (rules.TryGetValue(bit, out var rule)) 
        {
            return rule.Tiles;
        }
        return Array.Empty<SpriteTexture>();
    }

    public void AddRuleset(byte bit, Ruleset ruleset) 
    {
        rules[bit] = ruleset;
    }

    public void RemoveRuleset(byte bit) 
    {
        rules.Remove(bit);
    }
}

public struct Ruleset 
{
    public string Name;
    public byte[] Mask = new byte[9];
    public SpriteTexture[] Tiles;

    public Ruleset(string name, int tileCount) 
    {
        Name = name;
        Tiles = new SpriteTexture[tileCount];
    }
}

internal partial struct JsonRuleset : IDeserialize
{
    [TeuObject] [Name("name")] public string Name;
    [TeuObject] [Name("mask")] public byte[] Mask;
    [TeuObject] [Name("tiles")] public int[,] Tiles;
}