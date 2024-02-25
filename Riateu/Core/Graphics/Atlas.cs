using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using TeuJson;

namespace Riateu.Graphics;

/// <summary>
/// A class that contains all of the quads from a packed texture that can be retrieved
/// by a name.
/// </summary>
public class Atlas 
{
    /// <summary>
    /// An enum that specify a file type of an atlas.
    /// </summary>
    public enum FileType 
    {   /// 
        Json, 
        ///
        Bin 
    }
    /// <summary>
    /// A property configuration whether the nine patch feature should be enabled.
    /// </summary>
    public bool NinePatchEnabled { get; set; } 
    private bool ninePatchEnabled;

    private Dictionary<string, int> lookup = new();
    private Quad[] textures;

    /// <summary>
    /// A map to the id of a texture by string.
    /// </summary>
    public IReadOnlyDictionary<string, int> Lookup => lookup;
    /// <summary>
    /// A list of all textures in this atlas.
    /// </summary>
    public Quad[] Textures => textures;

    /// <summary>
    /// Retrieve a quad by name
    /// </summary>
    /// <returns>
    /// A <see cref="Riateu.Graphics.Quad"/> that is aligned to a specific quad 
    /// to the packed texture
    /// </returns>
    public Quad this[string name] => Get(name);

    private Atlas() {}

    /// <summary>
    /// A method that load and create an atlas from a file.
    /// </summary>
    /// <param name="path">A path to the atlas file</param>
    /// <param name="texture">A texture to use for an atlas</param>
    /// <param name="fileType">A file type that the atlas used</param>
    /// <param name="ninePatchEnabled">Whether the nine patch feature is enabled</param>
    /// <returns>An <see cref="Riateu.Graphics.Atlas"/></returns>
    public static Atlas LoadFromFile(string path, Texture texture, FileType fileType = FileType.Json, bool ninePatchEnabled = false) 
    {
        using var fs = File.OpenRead(path);
        return LoadFromStream(fs, texture, fileType, ninePatchEnabled);
    }

    /// <summary>
    /// A method that load and create an atlas from a file.
    /// </summary>
    /// <param name="stream">A stream containing the atlas file</param>
    /// <param name="texture">A texture to use for an atlas</param>
    /// <param name="fileType">A file type that the atlas used</param>
    /// <param name="ninePatchEnabled">Whether the nine patch feature is enabled</param>
    /// <returns>An <see cref="Riateu.Graphics.Atlas"/></returns>
    public static Atlas LoadFromStream(Stream stream, Texture texture, FileType fileType = FileType.Json, bool ninePatchEnabled = false) 
    {
        var atlas = new Atlas();
        atlas.ninePatchEnabled = ninePatchEnabled;
        switch (fileType) 
        {
        default:
            var val = JsonTextReader.FromStream(stream);
            var frames = val["frames"].AsJsonObject;
            var count = frames.Count;
            atlas.textures = new Quad[count];
            int textureID = -1;
            foreach (var kv in frames.Pairs) 
            {
                textureID++;
                var key = kv.Key;
                var value = kv.Value;
                int x = value["x"];
                int y = value["y"];
                int w = value["width"];
                int h = value["height"];
                atlas.lookup[key] = textureID;

                if (!value.Contains("nine_patch")) 
                {
                    var spriteTexture = new Quad(texture, new Rect(x, y, w, h));
                    atlas.textures[textureID] = spriteTexture;
                    continue;
                }

                JsonObject ninePatch = kv.Value["nine_patch"].AsJsonObject;
                int nx = ninePatch["x"];
                int ny = ninePatch["y"];
                int nw = ninePatch["w"];
                int nh = ninePatch["h"];
                // TODO add nine patch

                var ninePatchTexture = new Quad(texture, new Rect(x, y, w, h));
                atlas.textures[textureID] = ninePatchTexture;
            }
            return atlas;
        case FileType.Bin:
            var reader = new BinaryReader(stream);
            reader.ReadString();
            var length = reader.ReadUInt32();
            atlas.textures = new Quad[length];
            for (int i = 0; i < length; i++) 
            {
                var name = reader.ReadString();
                var x = (int)reader.ReadUInt32();
                var y = (int)reader.ReadUInt32();
                var w = (int)reader.ReadUInt32();
                var h = (int)reader.ReadUInt32();

                atlas.lookup[name] = i;
                if (!atlas.ninePatchEnabled) 
                {
                    var spriteTexture = new Quad(texture, new Rect(x, y, w, h));
                    atlas.textures[i] = spriteTexture;
                    continue;
                }
                var hasNinePatch = reader.ReadBoolean();
                Quad ninePatchTexture;
                if (!hasNinePatch) 
                {
                    ninePatchTexture = new Quad(texture, new Rect(x, y, w, h));
                }
                else 
                {
                    var nx = (int)reader.ReadUInt32();
                    var ny = (int)reader.ReadUInt32();
                    var nw = (int)reader.ReadUInt32();
                    var nh = (int)reader.ReadUInt32();
                    // TODO add nine patch here
                    ninePatchTexture = new Quad(texture, new Rect(x, y, w, h));
                }

                atlas.textures[i] = ninePatchTexture;
            }
            return atlas;
        }
    }
    
    /// <summary>
    /// Retrive a quad by a name.
    /// </summary>
    /// <param name="name">A name of the quad from the packed texture</param>
    /// <returns>
    /// A <see cref="Riateu.Graphics.Quad"/> that is aligned to a specific quad 
    /// to the packed texture
    /// </returns>
    public Quad Get(string name) 
    {
        return textures[lookup[name]];
    }

    /// <summary>
    /// Get a refrence to a quad by a name.
    /// </summary>
    /// <param name="name">A name of the quad from the packed texture</param>
    /// <returns>
    /// A reference to a <see cref="Riateu.Graphics.Quad"/>.
    /// </returns>
    public ref Quad GetRef(string name) 
    {
        ref var textureID = ref CollectionsMarshal.GetValueRefOrNullRef(lookup, name);
        if (Unsafe.IsNullRef(in textureID)) 
        {
            throw new System.Exception($"'{name}' is not found!");
        }
        return ref textures[textureID];
    }
}