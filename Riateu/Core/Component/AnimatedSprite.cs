using System;
using System.Collections.Generic;
using System.IO;
using Riateu.Graphics;
using TeuJson;

namespace Riateu.Components;

/// <summary>
/// A component used for rendering sprite within each frame.
/// </summary>
public class AnimatedSprite : GraphicsComponent
{
    private AnimationIndex frames;
    private string currentAnimationName = string.Empty;
    private int currentFrame;
    private double fps = 10;
    private double timer;
    private bool playing;
    private bool isLoop;

    /// <summary>
    /// The frame per seconds of all animation.
    /// </summary>
    public double FPS 
    {
        get => fps;
        set => fps = value;
    }

    /// <summary>
    /// Whether the animation should loop.
    /// </summary>
    public bool Loop 
    {
        get => isLoop;
        set => isLoop = value;
    }

    /// <summary>
    /// A state to check if the current animation is still playing.
    /// </summary>
    public bool IsPlaying => playing;

    /// <summary>
    /// A current animation name.
    /// </summary>
    public string CurrentAnimation => currentAnimationName;
    
    
    /// <summary>
    /// Initialize an empty <see cref="Riateu.Components.AnimatedSprite"/>. You might want to use 
    /// <see cref="Riateu.Components.AnimatedSprite.Create(Texture, AnimationIndex)"/> instead.
    /// </summary>
    /// <param name="texture">A texture to base on</param>
    public AnimatedSprite(Texture texture) : base(texture) {}

    /// <summary>
    /// Create an <see cref="Riateu.Components.AnimatedSprite"/> from a json path.
    /// </summary>
    /// <param name="atlasTexture">A texture of the sprite</param>
    /// <param name="atlas">An atlas contaning the id texture of the sprite</param>
    /// <param name="jsonPath">A path to json file</param>
    /// <returns>An <see cref="Riateu.Components.AnimatedSprite"/></returns>
    public static AnimatedSprite Create(Texture atlasTexture, Atlas atlas, string jsonPath)  
    {
        using var fs = File.OpenRead(jsonPath);
        return Create(atlasTexture, atlas, fs);
    }

    /// <summary>
    /// Create an <see cref="Riateu.Components.AnimatedSprite"/> from a json stream.
    /// </summary>
    /// <param name="atlasTexture">A texture of the sprite</param>
    /// <param name="atlas">An atlas contaning the id texture of the sprite</param>
    /// <param name="stream">A stream containing the json file</param>
    /// <returns>An <see cref="Riateu.Components.AnimatedSprite"/></returns>
    public static AnimatedSprite Create(Texture atlasTexture, Atlas atlas, Stream stream) 
    {
        var animSprite = new AnimatedSprite(atlasTexture);
        var json = JsonTextReader.FromStream(stream);
        var texture = json["texture"];
        var cycles = json["cycles"];

        var frames = new Dictionary<string, Animation>();
        var names = new List<string>();

        foreach (var cycle in cycles.Pairs) 
        {
            var animation = new Animation();
            var key = cycle.Key;
            var value = cycle.Value;

            var jsonFrames = value["frames"];
            bool loop = value.Contains("loop") && value["loop"];
            var count = jsonFrames.Count;

            var spriteTextures = new TextureQuad[count];
            for (int i = 0; i < count; i++) 
            {
                spriteTextures[i] = atlas[texture + "/" + jsonFrames[i].AsInt32];
            }

            animation.Frames = spriteTextures;
            animation.Loop = loop;

            frames[key] = animation;
            names.Add(key);
        }

        var animationIndex = new AnimationIndex(names, frames);

        animSprite.frames = animationIndex;
        return animSprite;
    }

    /// <summary>
    /// Create an <see cref="Riateu.Components.AnimatedSprite"/> from a map of frames.
    /// </summary>
    /// <param name="atlasTexture">A texture for the sprite</param>
    /// <param name="animationIndex">A map of frames</param>
    /// <returns>An <see cref="Riateu.Components.AnimatedSprite"/></returns>
    public static AnimatedSprite Create(Texture atlasTexture, AnimationIndex animationIndex) 
    {
        var animSprite = new AnimatedSprite(atlasTexture);
        animSprite.frames = animationIndex;
        return animSprite;
    }

    /// <summary>
    /// Play the animation by the name.
    /// </summary>
    /// <param name="name">The name of the animation</param>
    public void Play(string name) 
    {
        if (name == currentAnimationName)
            return;

        Set(ref frames[name].Frames[0]);
        currentAnimationName = name;
        playing = true;
        currentFrame = 0;
        timer = 0;
    }

    /// <summary>
    /// Stop the current animation
    /// </summary>
    public void Stop() 
    {
        currentAnimationName = string.Empty;
        playing = false;
    }

    /// <inheritdoc/>
    public override void Update(double delta)
    {
        if (!playing)
            return;

        var currentFrames = frames[currentAnimationName];
        isLoop = currentFrames.Loop;
        var intTimer = Math.Sign(timer);
        timer += delta * fps;
        currentFrame += intTimer;
        timer -= intTimer;

        if (currentFrame < currentFrames.Frames.Length)
        {
            Set(ref currentFrames.Frames[currentFrame]);
            return;
        }
        timer = 0;
        if (isLoop)
        {
            currentFrame = 0;
            Set(ref currentFrames.Frames[0]);
            return;
        }

        playing = false;
        currentAnimationName = string.Empty;
    }

    private void Set(ref TextureQuad texture) 
    {
        SpriteTexture = texture;
    }

    /// <inheritdoc/>
    public override void Draw(Batch draw)
    {
        draw.Draw(SpriteTexture, Entity.Transform.Position, Color.White);
    }

    /// <summary>
    /// An animation struct containing the list of textures and attributes.
    /// </summary>
    public struct Animation 
    {
        /// <summary>
        /// A list of textures to animate with.
        /// </summary>
        public TextureQuad[] Frames;
        /// <summary>
        /// Whether to tell if the animation should loop when it play this animation.
        /// </summary>
        public bool Loop;
    }
}
