using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using TeuJson;
using TeuJson.Attributes;

namespace Riateu.Graphics;

/// <summary>
/// A class that defines the tileset and its rules. 
/// </summary>
public class Tileset 
{
    private Texture tilesetTexture;
    private Spritesheet spritesheet;
    private Dictionary<byte, Ruleset> rules = new();

    /// <summary>
    /// An initialization for this class.
    /// </summary>
    /// <param name="tileset">A texture of this <see cref="Riateu.Graphics.Tileset"/></param>
    /// <param name="atlasTexture">A quad of this <see cref="Riateu.Graphics.Tileset"/></param>
    /// <param name="tileWidth">A tile width of this <see cref="Riateu.Graphics.Tileset"/></param>
    /// <param name="tileHeight">A tile height of this <see cref="Riateu.Graphics.Tileset"/></param>
    public Tileset(
        Texture tileset, 
        TextureQuad atlasTexture,
        int tileWidth,
        int tileHeight) 
    {
        tilesetTexture = tileset;
        spritesheet = new Spritesheet(tileset, atlasTexture, tileWidth, tileHeight);
    }

    /// <summary>
    /// Create a tileset from a json rules path. 
    /// </summary>
    /// <param name="rulesPath">A path to json</param>
    /// <param name="texture">A texture will be used for the tileset</param>
    /// <param name="atlas">An atlas containing that tileset</param>
    /// <param name="jsonType">Specify what json type is the file</param>
    /// <returns>A created tileset</returns>
    public static Tileset Create(string rulesPath, Texture texture, Atlas atlas, JsonType jsonType = JsonType.Json) 
    {
        using var fs = File.OpenRead(rulesPath);
        return Create(fs, texture, atlas, jsonType);
    }

    /// <summary>
    /// Create a tileset from a stream. 
    /// </summary>
    /// <param name="rulesStream">A stream containing json</param>
    /// <param name="texture">A texture will be used for the tileset</param>
    /// <param name="atlas">An atlas containing that tileset</param>
    /// <param name="jsonType">Specify what json type is the file</param>
    /// <returns>A created tileset</returns>
    public static Tileset Create(Stream rulesStream, Texture texture, Atlas atlas, JsonType jsonType = JsonType.Json) 
    {
        JsonValue json = jsonType == JsonType.Json 
            ? JsonTextReader.FromStream(rulesStream) 
            : JsonBinaryReader.FromStream(rulesStream);
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
            var ruleset = new Ruleset(jsonRule.Name, jsonRule.Mask, jsonRule.Tiles.GetLength(0));

            for (int j = 0; j < jsonRule.Tiles.GetLength(0); j++) 
            {
                var x = jsonRule.Tiles[j, 0] - 1;
                var y = jsonRule.Tiles[j, 1] - 1;
                ruleset.Tiles[j] = tileset.spritesheet.GetTexture(x, y);
            }

            byte bit = 0;

            bit += (byte)(ruleset.Mask[0] * 1 << 0);
            bit += (byte)(ruleset.Mask[1] * 1 << 1);
            bit += (byte)(ruleset.Mask[2] * 1 << 2);
            bit += (byte)(ruleset.Mask[3] * 1 << 3);
            bit += (byte)(ruleset.Mask[5] * 1 << 4);
            bit += (byte)(ruleset.Mask[6] * 1 << 5);
            bit += (byte)(ruleset.Mask[7] * 1 << 6);
            bit += (byte)(ruleset.Mask[8] * 1 << 7);
            
            tileset.rules[bit] = ruleset;
        }
        return tileset;
    }

    /// <summary>
    /// Get all of the tiles from this ruleset by bit.
    /// </summary>
    /// <param name="bit">A bit of the ruleset</param>
    /// <returns>A list of tile textures</returns>
    public TextureQuad[] GetTilesFromBit(byte bit) 
    {
        if (rules.TryGetValue(bit, out var rule)) 
        {
            return rule.Tiles;
        }
        return Array.Empty<TextureQuad>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TextureQuad GetTile(int tile) 
    {
        return spritesheet[tile];
    }

    /// <summary>
    /// Add a ruleset for this tileset.
    /// </summary>
    /// <param name="bit">A bit mask of the ruleset</param>
    /// <param name="ruleset">A ruleset to add or insert with the tileset</param>
    public void AddRuleset(byte bit, Ruleset ruleset) 
    {
        rules[bit] = ruleset;
    }

    /// <summary>
    /// Remove a ruleset from this tileset.
    /// </summary>
    /// <param name="bit">A bit mask of the ruleset</param>
    public void RemoveRuleset(byte bit) 
    {
        rules.Remove(bit);
    }
}

/// <summary>
/// A struct that defines a tileset rules based on the mask.
/// </summary>
public struct Ruleset 
{
    /// <summary>
    /// The name of the ruleset.
    /// </summary>
    public string Name;
    /// <summary>
    /// The mask of the ruleset.
    /// </summary>
    public byte[] Mask = new byte[9];
    /// <summary>
    /// The tiles that will be randomly chosen.
    /// </summary>
    public TextureQuad[] Tiles;

    /// <summary>
    /// An initialization for ruleset
    /// </summary>
    /// <param name="name">The name of the ruleset</param>
    /// <param name="mask">The mask of the ruleset</param>
    /// <param name="tileCount">How many tiles does this ruleset has?</param>
    public Ruleset(string name, string mask, int tileCount) 
    {
        Name = name;
        Tiles = new TextureQuad[tileCount];
        var strMask = mask.AsSpan();

        for (int i = 0; i < strMask.Length; i++) 
        {
            Mask[i] = strMask[i] == 'X' ? (byte)0 : (byte)1;
        }
    }
}

internal partial struct JsonRuleset : IDeserialize
{
    [TeuObject] [Name("name")] public string Name;
    [TeuObject] [Name("mask")] public string Mask;
    [TeuObject] [Name("tiles")] public int[,] Tiles;
}