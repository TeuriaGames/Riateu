using System.Collections.Generic;
using Riateu.Graphics;

namespace Riateu.ECS.Components;

public struct SpriteRenderer(Quad texture) 
{
    public Quad Texture = texture;
}

public struct AnimatedSprite(uint animationID, float fps = 10) 
{
    public uint AnimationID = animationID;
    public uint SubAnimID;
    public int CurrentFrame;
    public float FPS = fps;
    public float TimeLeft;
    public bool Playing;

    public void Play(uint index) 
    {
        if (SubAnimID == index) 
        {
            return;
        }
        Playing = true;
        SubAnimID = index;
        CurrentFrame = 0;
    }
}

public class AnimationStorage 
{
    private List<Dictionary<string, Animation>> animations = new List<Dictionary<string, Animation>>();
    private Dictionary<string, uint> animationID = new Dictionary<string, uint>();
    private uint count;

    public void AddAnimation(string name, Dictionary<string, Animation> animations) 
    {
        animationID.Add(name, count++);
        this.animations.Add(animations);
    }

    public uint GetAnimationID(string name) 
    {
        return animationID[name];
    }

    public Dictionary<string, Animation> GetAnimation(string name) 
    {
        return animations[(int)animationID[name]];
    }
}

public struct Animation 
{
    /// <summary>
    /// A list of textures to animate with.
    /// </summary>
    public Quad[] Frames;
    /// <summary>
    /// Whether to tell if the animation should loop when it play this animation.
    /// </summary>
    public bool Loop;
}