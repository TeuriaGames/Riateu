using System.Collections.Generic;
using System.IO;
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
    private Dictionary<string, SpriteTexture> textures = new();

    public IReadOnlyDictionary<string, SpriteTexture> Textures => textures;

    public SpriteTexture this[string name] => Get(name);

    private Atlas() {}

    public static Atlas LoadFromFile(string path, Texture texture, FileType fileType = FileType.Json) 
    {
        using var fs = File.OpenRead(path);
        return LoadFromStream(fs, texture, fileType);
    }

    public static Atlas LoadFromStream(Stream fs, Texture texture, FileType fileType = FileType.Json) 
    {
        var atlas = new Atlas();
        switch (fileType) 
        {
        default:
            var val = JsonTextReader.FromStream(fs);
            var frames = val["frames"].AsJsonObject;
            foreach (var kv in frames.Pairs) 
            {
                var key = kv.Key;
                var value = kv.Value;
                int x = value["x"];
                int y = value["y"];
                int w = value["width"];
                int h = value["height"];

                if (value.Contains("nine_patch")) 
                {
                    var spriteTexture = new SpriteTexture(texture, new Rect(x, y, w, h));
                    atlas.textures[key] = spriteTexture;
                    continue;
                }

                JsonObject ninePatch = kv.Value["nine_patch"].AsJsonObject;
                int nx = ninePatch["x"];
                int ny = ninePatch["y"];
                int nw = ninePatch["w"];
                int nh = ninePatch["h"];
                // TODO add nine patch

                var ninePatchTexture = new SpriteTexture(texture, new Rect(x, y, w, h));
                atlas.textures[key] = ninePatchTexture;
            }
            return atlas;
        case FileType.Bin:
            var reader = new BinaryReader(fs);
            reader.ReadString();
            var length = reader.ReadUInt32();
            for (uint i = 0; i < length; i++) 
            {
                var name = reader.ReadString();
                var x = (int)reader.ReadUInt32();
                var y = (int)reader.ReadUInt32();
                var w = (int)reader.ReadUInt32();
                var h = (int)reader.ReadUInt32();
                if (!atlas.NinePatchEnabled) 
                {
                    var spriteTexture = new SpriteTexture(texture, new Rect(x, y, w, h));
                    atlas.textures[name] = spriteTexture;
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

                atlas.textures[name] = ninePatchTexture;
            }
            return atlas;
        }
    }
    

    public SpriteTexture Get(string name) 
    {
        return textures[name];
    }

    public ref SpriteTexture GetRef(string name) 
    {
        ref var texture = ref CollectionsMarshal.GetValueRefOrNullRef(textures, name);
        if (Unsafe.IsNullRef(in texture)) 
        {
            throw new System.Exception($"'{name}' is not found!");
        }
        return ref texture;
    }
}