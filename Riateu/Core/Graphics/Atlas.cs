using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using TeuJson;

namespace Riateu.Graphics;

public class Atlas 
{
    public bool NinePatchEnabled;
    public enum FileType 
    {
        Json,
        Bin
    }
    private Dictionary<string, int> lookup = new();
    private SpriteTexture[] textures;

    public IReadOnlyDictionary<string, int> Lookup => lookup;
    public SpriteTexture[] Textures => textures;

    public SpriteTexture this[string name] => Get(name);

    private Atlas() {}

    public static Atlas LoadFromFile(string path, Texture texture, FileType fileType = FileType.Json, bool ninePatchEnabled = false) 
    {
        using var fs = File.OpenRead(path);
        return LoadFromStream(fs, texture, fileType, ninePatchEnabled);
    }

    public static Atlas LoadFromStream(Stream fs, Texture texture, FileType fileType = FileType.Json, bool ninePatchEnabled = false) 
    {
        var atlas = new Atlas();
        atlas.NinePatchEnabled = ninePatchEnabled;
        switch (fileType) 
        {
        default:
            var val = JsonTextReader.FromStream(fs);
            var frames = val["frames"].AsJsonObject;
            var count = frames.Count;
            atlas.textures = new SpriteTexture[count];
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
                    var spriteTexture = new SpriteTexture(texture, new Rect(x, y, w, h));
                    atlas.textures[textureID] = spriteTexture;
                    continue;
                }

                JsonObject ninePatch = kv.Value["nine_patch"].AsJsonObject;
                int nx = ninePatch["x"];
                int ny = ninePatch["y"];
                int nw = ninePatch["w"];
                int nh = ninePatch["h"];
                // TODO add nine patch

                var ninePatchTexture = new SpriteTexture(texture, new Rect(x, y, w, h));
                atlas.textures[textureID] = ninePatchTexture;
            }
            return atlas;
        case FileType.Bin:
            var reader = new BinaryReader(fs);
            reader.ReadString();
            var length = reader.ReadUInt32();
            atlas.textures = new SpriteTexture[length];
            for (int i = 0; i < length; i++) 
            {
                var name = reader.ReadString();
                var x = (int)reader.ReadUInt32();
                var y = (int)reader.ReadUInt32();
                var w = (int)reader.ReadUInt32();
                var h = (int)reader.ReadUInt32();

                atlas.lookup[name] = i;
                if (!atlas.NinePatchEnabled) 
                {
                    var spriteTexture = new SpriteTexture(texture, new Rect(x, y, w, h));
                    atlas.textures[i] = spriteTexture;
                    continue;
                }
                var hasNinePatch = reader.ReadBoolean();
                SpriteTexture ninePatchTexture;
                if (!hasNinePatch) 
                {
                    ninePatchTexture = new SpriteTexture(texture, new Rect(x, y, w, h));
                }
                else 
                {
                    var nx = (int)reader.ReadUInt32();
                    var ny = (int)reader.ReadUInt32();
                    var nw = (int)reader.ReadUInt32();
                    var nh = (int)reader.ReadUInt32();
                    // TODO add nine patch here
                    ninePatchTexture = new SpriteTexture(texture, new Rect(x, y, w, h));
                }

                atlas.textures[i] = ninePatchTexture;
            }
            return atlas;
        }
    }
    

    public SpriteTexture Get(string name) 
    {
        return textures[lookup[name]];
    }

    public ref SpriteTexture GetRef(string name) 
    {
        ref var textureID = ref CollectionsMarshal.GetValueRefOrNullRef(lookup, name);
        if (Unsafe.IsNullRef(in textureID)) 
        {
            throw new System.Exception($"'{name}' is not found!");
        }
        return ref textures[textureID];
    }
}