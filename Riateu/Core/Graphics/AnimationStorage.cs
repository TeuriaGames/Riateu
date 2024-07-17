using System.Collections.Generic;
using System.IO;
using Riateu.Components;
using TeuJson;

namespace Riateu.Graphics;

public class AnimationIndex 
{
    public List<string> AnimationNames;
    public Dictionary<string, AnimatedSprite.Animation> Animations;

    public AnimationIndex(List<string> names, Dictionary<string, AnimatedSprite.Animation> animations) 
    {
        AnimationNames = names;
        Animations = animations;
    }

    public AnimatedSprite.Animation this[string name] 
    {
        get => Animations[name];
    }

    public AnimatedSprite.Animation this[uint index] 
    {
        get => Animations[AnimationNames[(int)index]];
    }
}

/// <summary>
/// A class that stores all of the frames animation from an <see cref="Riateu.Graphics.Atlas"/>.
/// </summary>
public class AnimationStorage 
{
    private List<AnimationIndex> animations = new List<AnimationIndex>();
    private Dictionary<string, uint> animationIDs = new Dictionary<string, uint>();
    private uint count;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public AnimationIndex this[string name] => Load(name);

    public void Add(string name, AnimationIndex index) 
    {
        animationIDs[name] = count++;
        animations.Add(index);
    }

    /// <summary>
    /// Create a storage from a json path that contains all of the animation frames inside.
    /// </summary>
    /// <param name="path">A path to animation storage json</param>
    /// <param name="atlas">An <see cref="Riateu.Graphics.Atlas"/> to use</param>
    /// <param name="jsonType">Specify what json type is the file</param>
    /// <returns>A storage for all animation</returns>
    public static AnimationStorage Create(string path, Atlas atlas, JsonType jsonType = JsonType.Json) 
    {
        using var fs = File.OpenRead(path);
        return Create(fs, atlas, jsonType);
    }

    /// <summary>
    /// Create a storage from a json path that contains all of the animation frames inside.
    /// </summary>
    /// <param name="stream">A stream containing the json contents</param>
    /// <param name="atlas">An <see cref="Riateu.Graphics.Atlas"/> to use</param>
    /// <param name="jsonType">Specify what json type is the file</param>
    /// <returns>A storage for all animation</returns>
    public static AnimationStorage Create(Stream stream, Atlas atlas, JsonType jsonType = JsonType.Json) 
    {
        var animations = new AnimationStorage();
        JsonValue jsonBank;
        if (jsonType == JsonType.Bin) 
        {
            jsonBank = JsonBinaryReader.FromStream(stream);
        }
        else 
        {
            jsonBank = JsonTextReader.FromStream(stream);
        }

        foreach (var (k, v) in jsonBank.Pairs) 
        {
            var spriteName = k;
            var json = jsonBank[k];
            var frames = new Dictionary<string, AnimatedSprite.Animation>();
            var names = new List<string>();

            var cycles = json["cycles"];

            foreach (var cycle in cycles.Pairs) 
            {
                var animation = new AnimatedSprite.Animation();
                var key = cycle.Key;
                var value = cycle.Value;

                var jsonFrames = value["frames"];
                bool loop = value.Contains("loop") && value["loop"];
                var count = jsonFrames.Count;

                var spriteTextures = new Quad[count];
                for (int i = 0; i < count; i++) 
                {
                    spriteTextures[i] = atlas[spriteName + "/" + jsonFrames[i].AsInt32];
                }

                animation.Frames = spriteTextures;
                animation.Loop = loop;

                frames[key] = animation;
                names.Add(key);
            }

            var animationIndex = new AnimationIndex(names, frames);
            animations.Add(spriteName, animationIndex);
        }
        return animations;
    }

    /// <summary>
    /// Load an animation frame by name from a storage.
    /// </summary>
    /// <param name="anim">A name or id of the animation</param>
    /// <returns>An animation frame</returns>
    public AnimationIndex Load(string anim) 
    {
        return animations[(int)animationIDs[anim]];
    }

    public AnimationIndex Load(uint anim) 
    {
        return animations[(int)anim];
    }

    public uint GetID(string anim) 
    {
        return animationIDs[anim];
    }
}