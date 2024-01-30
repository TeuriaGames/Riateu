using System.Collections.Generic;
using System.IO;
using Riateu.Components;
using TeuJson;

namespace Riateu.Graphics;

/// <summary>
/// A class that stores all of the frames animation from an <see cref="Riateu.Graphics.Atlas"/>.
/// </summary>
public class AnimationStorage 
{
    private Dictionary<string, Dictionary<string, AnimatedSprite.Animation>> animations = new();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, AnimatedSprite.Animation> this[string name] => Load(name);

    /// <summary>
    /// Create a storage from a json path that contains all of the animation frames inside.
    /// </summary>
    /// <param name="path">A path to animation storage json</param>
    /// <param name="atlas">An <see cref="Riateu.Graphics.Atlas"/> to use</param>
    /// <returns>A storage for all animation</returns>
    public static AnimationStorage Create(string path, Atlas atlas) 
    {
        using var fs = File.OpenRead(path);
        return Create(fs, atlas);
    }

    /// <summary>
    /// Create a storage from a json path that contains all of the animation frames inside.
    /// </summary>
    /// <param name="stream">A stream containing the json contents</param>
    /// <param name="atlas">An <see cref="Riateu.Graphics.Atlas"/> to use</param>
    /// <returns>A storage for all animation</returns>
    public static AnimationStorage Create(Stream stream, Atlas atlas) 
    {
        var animations = new AnimationStorage();
        var jsonBank = JsonTextReader.FromStream(stream);
        var dict = new Dictionary<string, Dictionary<string, AnimatedSprite.Animation>>();

        foreach (var (k, v) in jsonBank.Pairs) 
        {
            var spriteName = k;
            var json = jsonBank[k];
            var frames = new Dictionary<string, AnimatedSprite.Animation>();

            var cycles = json["cycles"];

            foreach (var cycle in cycles.Pairs) 
            {
                var animation = new AnimatedSprite.Animation();
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

    /// <summary>
    /// Load an animation frame by name from a storage.
    /// </summary>
    /// <param name="anim">A name or id of the animation</param>
    /// <returns>An animation frame</returns>
    public Dictionary<string, AnimatedSprite.Animation> Load(string anim) 
    {
        return animations[anim];
    }
}