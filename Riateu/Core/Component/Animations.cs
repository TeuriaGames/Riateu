using System.Collections.Generic;
using System.IO;
using Riateu.Components;
using Riateu.Graphics;
using TeuJson;

namespace Riateu;

public class Animations 
{
    private Dictionary<string, Dictionary<string, Animation>> animations = new();

    public Dictionary<string, Animation> this[string name] => Load(name);

    public static Animations Create(string path, Atlas atlas) 
    {
        using var fs = File.OpenRead(path);
        return Create(fs, atlas);
    }

    public static Animations Create(Stream stream, Atlas atlas) 
    {
        var animations = new Animations();
        var jsonBank = JsonTextReader.FromStream(stream);
        var dict = new Dictionary<string, Dictionary<string, Animation>>();

        foreach (var (k, v) in jsonBank.Pairs) 
        {
            var spriteName = k;
            var json = jsonBank[k];
            var frames = new Dictionary<string, Animation>();

            var cycles = json["cycles"];

            foreach (var cycle in cycles.Pairs) 
            {
                var animation = new Animation();
                var key = cycle.Key;
                var value = cycle.Value;

                var jsonFrames = value["frames"];
                bool loop = value.Contains("loop") && value["loop"];
                var count = jsonFrames.Count;

                var spriteTextures = new SpriteTexture[count];
                for (int i = 0; i < count; i++) 
                {
                    spriteTextures[i] = atlas[spriteName + "/" + jsonFrames[i].AsInt32];
                }

                animation.Frames = spriteTextures;
                animation.Loop = loop;

                frames[key] = animation;
            }
            dict.Add(spriteName, frames);
        }
        animations.animations = dict;
        return animations;
    }

    public Dictionary<string, Animation> Load(string anim) 
    {
        return animations[anim];
    }
}