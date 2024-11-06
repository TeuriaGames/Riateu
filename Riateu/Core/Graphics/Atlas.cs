using System.IO;
using System.Collections.Generic;
using Riateu.Content;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Linq;
using System;

namespace Riateu.Graphics;

/// <summary>
/// A class that contains all of the quads from a packed texture that can be retrieved
/// by a name.
/// </summary>
public class Atlas : IAssets, IDisposable
{
    /// <summary>
    /// A base texture of an atlas.
    /// </summary>
    public Texture BaseTexture { get; private set; }
    /// <summary>
    /// A property configuration whether the nine patch feature should be enabled.
    /// </summary>
    public bool NinePatchEnabled { get; set; } 
    private bool ninePatchEnabled;

    private Dictionary<string, TextureQuad> textures = new();

    /// <summary>
    /// A map to the quad by string.
    /// </summary>
    public IReadOnlyDictionary<string, TextureQuad> Lookup => textures;

    /// <summary>
    /// Retrieve a quad by name
    /// </summary>
    /// <returns>
    /// A <see cref="Riateu.Graphics.TextureQuad"/> that is aligned to a specific quad 
    /// to the packed texture
    /// </returns>
    public TextureQuad this[string name] => Get(name);

    /// <summary>
    /// Creates an empty atlas.
    /// </summary>
    public Atlas() {}

    /// <summary>
    /// Creates an empty atlas.
    /// </summary>
    /// <param name="ninePatchEnabled">Whether the nine patch feature is enabled</param>
    public Atlas(bool ninePatchEnabled) 
    {
        this.ninePatchEnabled = ninePatchEnabled;
    }

    /// <summary>
    /// A method that load and create an <see cref="Riateu.Graphics.Atlas"/> from a file.
    /// </summary>
    /// <param name="path">A path to the atlas file</param>
    /// <param name="texture">A texture to use for an atlas</param>
    /// <param name="fileType">A file type that the atlas used</param>
    /// <param name="ninePatchEnabled">Whether the nine patch feature is enabled</param>
    /// <returns>An <see cref="Riateu.Graphics.Atlas"/></returns>
    public static Atlas LoadFromFile(string path, Texture texture, JsonType fileType = JsonType.Json, bool ninePatchEnabled = false) 
    {
        using var fs = File.OpenRead(path);
        return LoadFromStream(fs, texture, fileType, ninePatchEnabled);
    }

    /// <summary>
    /// A method that load and create an <see cref="Riateu.Graphics.Atlas"/> from a <see cref="Riateu.Content.Packer{T}"/>.
    /// </summary>
    /// <param name="uploader">
    /// A <see cref="Riateu.Graphics.ResourceUploader"/> that creates and uploads a <see cref="Riateu.Graphics.Texture"/> to GPU.
    /// </param>
    /// <param name="items">A list of packed items that has been packed by <see cref="Riateu.Content.Packer{T}"/> already</param>
    /// <param name="size">A size of a result packed from a <see cref="Riateu.Content.Packer{T}"/></param>
    /// <returns>An <see cref="Riateu.Graphics.Atlas"/></returns>
    public static Atlas LoadFromPacker(ResourceUploader uploader, List<Packer<AtlasItem>.PackedItem> items, Point size) 
    {
        var atlas = new Atlas();
        Image image = new Image(size.X, size.Y);

        for (int i = 0; i < items.Count; i++) 
        {
            Packer<AtlasItem>.PackedItem item = items[i];
            item.Data.Image.Premultiply();
            image.CopyFrom(item.Data.Image, item.Rect.X, item.Rect.Y);
            atlas.textures.Add(item.Data.Name, new TextureQuad(
                size, new Rectangle(item.Rect.X, item.Rect.Y, item.Rect.Width, item.Rect.Height)));
            item.Data.Image.Dispose();
        }

        Texture texture = uploader.CreateTexture2D(image.Pixels, (uint)image.Width, (uint)image.Height);
        atlas.BaseTexture = texture;
        return atlas;
    }

    /// <summary>
    /// A method that load and create an <see cref="Riateu.Graphics.Atlas"/> from a file.
    /// </summary>
    /// <param name="stream">A stream containing the <see cref="Riateu.Graphics.Atlas"/> file</param>
    /// <param name="texture">A <see cref="Riateu.Graphics.Texture"/> to use for an atlas</param>
    /// <param name="fileType">A file type that the <see cref="Riateu.Graphics.Atlas"/> used</param>
    /// <param name="ninePatchEnabled">Whether the nine patch feature is enabled</param>
    /// <returns>An <see cref="Riateu.Graphics.Atlas"/></returns>
    public static Atlas LoadFromStream(Stream stream, Texture texture, JsonType fileType = JsonType.Json, bool ninePatchEnabled = false) 
    {
        var atlas = new Atlas();
        atlas.BaseTexture = texture;
        atlas.ninePatchEnabled = ninePatchEnabled;
        switch (fileType) 
        {
        default:
            ClutteredAtlasData atlasData = JsonSerializer.Deserialize<ClutteredAtlasData>(stream, ClutteredAtlasDataContext.Default.ClutteredAtlasData);
            var frames = atlasData.Frames;
            var count = atlasData.Frames.Count;
            foreach (var kv in frames)
            {
                var key = kv.Key;
                var value = kv.Value;
                int x = value.X;
                int y = value.Y;
                int w = value.Width;
                int h = value.Height;

                if (value.NinePatch is not ClutteredNinePatch ninePatch) 
                {
                    var spriteTexture = new TextureQuad(texture, new Rectangle(x, y, w, h));
                    atlas.textures[key] = spriteTexture;
                    continue;
                }

                int nx = ninePatch.X;
                int ny = ninePatch.Y;
                int nw = ninePatch.W;
                int nh = ninePatch.H;
                // TODO add nine patch

                var ninePatchTexture = new TextureQuad(texture, new Rectangle(x, y, w, h));
                atlas.textures[key] = ninePatchTexture;
            }
            return atlas;
        case JsonType.Bin:
            var reader = new BinaryReader(stream);
            reader.ReadString();
            var length = reader.ReadUInt32();
            for (int i = 0; i < length; i++) 
            {
                var name = reader.ReadString();
                var x = (int)reader.ReadUInt32();
                var y = (int)reader.ReadUInt32();
                var w = (int)reader.ReadUInt32();
                var h = (int)reader.ReadUInt32();

                if (!atlas.ninePatchEnabled) 
                {
                    var spriteTexture = new TextureQuad(texture, new Rectangle(x, y, w, h));
                    atlas.textures[name] = spriteTexture;
                    continue;
                }
                var hasNinePatch = reader.ReadBoolean();
                TextureQuad ninePatchTexture;
                if (!hasNinePatch) 
                {
                    ninePatchTexture = new TextureQuad(texture, new Rectangle(x, y, w, h));
                }
                else 
                {
                    var nx = (int)reader.ReadUInt32();
                    var ny = (int)reader.ReadUInt32();
                    var nw = (int)reader.ReadUInt32();
                    var nh = (int)reader.ReadUInt32();
                    // TODO add nine patch here
                    ninePatchTexture = new TextureQuad(texture, new Rectangle(x, y, w, h));
                }

                atlas.textures[name] = ninePatchTexture;
            }
            return atlas;
        }
    }
    /// <summary>
    /// Add a <see cref="Riateu.Graphics.TextureQuad"/> to the atlas.
    /// </summary>
    /// <param name="name">A name of the quad from the packed texture</param>
    /// <param name="quad">A <see cref="Riateu.Graphics.TextureQuad"/> from a texture with its specific position and dimension</param>
    public void Add(string name, TextureQuad quad) 
    {
        textures[name] = quad;
    }
    
    /// <summary>
    /// Retrive a quad by a name.
    /// </summary>
    /// <param name="name">A name of the quad from the packed texture</param>
    /// <returns>
    /// A <see cref="Riateu.Graphics.TextureQuad"/> that is aligned to a specific quad 
    /// to the packed texture
    /// </returns>
    public TextureQuad Get(string name) 
    {
        return textures[name];
    }

    public void Dispose()
    { 
        BaseTexture.Dispose();
    }

    public static implicit operator Texture(Atlas atlas) => atlas.BaseTexture;
}


[JsonSerializable(typeof(ClutteredAtlasData))]
[JsonSerializable(typeof(ClutteredFrames))]
[JsonSerializable(typeof(ClutteredNinePatch))]
internal partial class ClutteredAtlasDataContext : JsonSerializerContext
{

}

internal struct ClutteredAtlasData 
{
    [JsonPropertyName("frames")]
    public Dictionary<string, ClutteredFrames> Frames { get; set; }
}

internal struct ClutteredFrames 
{
    [JsonPropertyName("x")]
    public int X { get; set; }
    [JsonPropertyName("y")]
    public int Y { get; set; }
    [JsonPropertyName("width")]
    public int Width { get; set; }
    [JsonPropertyName("height")]
    public int Height { get; set; }
    [JsonPropertyName("nine_patch")]
    public ClutteredNinePatch? NinePatch { get; set; }
}

internal struct ClutteredNinePatch 
{
    [JsonPropertyName("x")]
    public int X { get; set; }
    [JsonPropertyName("y")]
    public int Y { get; set; }
    [JsonPropertyName("w")]
    public int W { get; set; }
    [JsonPropertyName("h")]
    public int H { get; set; }
}