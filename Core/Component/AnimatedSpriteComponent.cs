using System;
using System.Collections.Generic;
using System.IO;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;
using TeuJson;

namespace Riateu.Components;

public class AnimatedSprite : GraphicsComponent
{
    private Dictionary<string, SpriteTexture[]> frames;
    private string currentAnimationName;
    private int currentFrame;
    private double fps = 10;
    private double timer;
    private bool playing;
    private bool isLoop;

    public double FPS 
    {
        get => fps;
        set => fps = value;
    }

    public bool Loop 
    {
        get => isLoop;
        set => isLoop = value;
    }

    public bool IsPlaying => playing;
    
    
    private AnimatedSprite(Texture texture) : base(texture) {}

    public static AnimatedSprite Create(Texture atlasTexture, Atlas atlas, string jsonPath)  
    {
        using var fs = File.OpenRead(jsonPath);
        return Create(atlasTexture, atlas, fs);
    }

    public static AnimatedSprite Create(Texture atlasTexture, Atlas atlas, Stream stream) 
    {
        var animSprite = new AnimatedSprite(atlasTexture);
        var json = JsonTextReader.FromStream(stream);
        var texture = json["texture"];
        var cycles = json["cycles"];

        var frames = new Dictionary<string, SpriteTexture[]>();

        foreach (var cycle in cycles.Pairs) 
        {
            var key = cycle.Key;
            var value = cycle.Value;

            var jsonFrames = value["frames"];
            var count = jsonFrames.Count;

            var spriteTextures = new SpriteTexture[count];
            for (int i = 0; i < count; i++) 
            {
                spriteTextures[i] = atlas[texture + "/" + jsonFrames[i].AsInt32];
            }

            frames[key] = spriteTextures;
        }

        animSprite.frames = frames;
        return animSprite;
    }

    public static AnimatedSprite Create(Texture atlasTexture, Dictionary<string, SpriteTexture[]> frames) 
    {
        var animSprite = new AnimatedSprite(atlasTexture);
        animSprite.frames = frames;
        return animSprite;
    }

    public static AnimatedSprite Create(Texture atlasTexture) 
    {
        var animSprite = new AnimatedSprite(atlasTexture);
        animSprite.frames = new Dictionary<string, SpriteTexture[]>();
        return animSprite;
    }

    public void Add(string name, SpriteTexture[] textures) 
    {
        frames[name] = textures;
    }

    public void Play(string frame) 
    {
        if (frame == currentAnimationName)
            return;
        Set(ref frames[frame][0]);
        currentAnimationName = frame;
        playing = true;
    }

    public void Stop() 
    {
        currentAnimationName = string.Empty;
        playing = false;
    }

    public override void Update(double delta)
    {
        if (!playing)
            return;

        var currentFrames = frames[currentAnimationName];
        var intTimer = Math.Sign(timer);
        timer += delta * fps;
        currentFrame += intTimer;
        timer -= intTimer;

        if (currentFrame < currentFrames.Length)
        {
            Set(ref currentFrames[currentFrame]);
            return;
        }
        timer = 0;
        if (isLoop)
        {
            currentFrame = 0;
            Set(ref currentFrames[0]);
            return;
        }

        playing = false;
        currentAnimationName = string.Empty;
    }

    private void Set(ref SpriteTexture texture) 
    {
        if (SpriteTexture.Equals(texture))
            return;
        SpriteTexture = texture;
    }

    public override void Draw(Batch spriteBatch)
    {
        spriteBatch.Add(SpriteTexture, BaseTexture, GameContext.GlobalSampler, 
            Vector2.Zero, 
            Entity.Transform.WorldMatrix);
    }
}