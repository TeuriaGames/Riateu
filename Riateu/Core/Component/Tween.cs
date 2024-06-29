using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoonWorks.Math;

namespace Riateu.TWTween;

// https://github.com/JamesMcMahon/monocle-engine/blob/759c15ad6a59c4855db36fd0dfc1970cef9607a5/Monocle/Components/Logic/Tween.cs
// https://github.com/JamesMcMahon/monocle-engine/blob/759c15ad6a59c4855db36fd0dfc1970cef9607a5/Monocle/Util/Ease.cs

public class Tween : Component
{
    public enum TweenMode { Persistent, OneShot, Loop, YoyoLoop }

    public Ease.Easer Easer { get; set; }
    public TweenMode Mode { get; internal set; }
    public float Duration { get; internal set; }
    public float Delay { get; internal set; }
    public bool Reverse { get; internal set; }
    public float TimeLeft { get; internal set; }
    public float Progress { get; internal set; }
    public float Value { get; internal set; }
    private float lastDelay;


    public Action<Tween> OnReady;
    public Action<Tween> OnProcess;
    public Action<Tween> OnEnd;

    private static Stack<Tween> cached = new Stack<Tween>();


    internal Tween() {}

    public static Tween Create(Entity entity, TweenMode mode, Ease.Easer easer = null, float duration = 1f, bool start = false) 
    {
        Tween tween;
        if (cached.Count == 0)
            tween = new Tween();
        else
            tween = cached.Pop();
        tween.Mode = mode;
        tween.Easer = easer;
        tween.Duration = duration;
        tween.Active = false;
        
        entity.AddComponent(tween);
        if (start) 
            tween.Start();
        
        return tween;
    }

    public override void Added(Entity entity)
    {
        Entity = entity;
    }

    public void Reset() 
    {
        TimeLeft = Duration;
        Value = Progress = Reverse ? 1 : 0;
        Progress = 0;
    }

    public void Stop() 
    {
        Reset();
        Active = false;
        OnEnd?.Invoke(this);
    }

    public override void Update(double delta)
    {
        if (Delay > 0) 
        {
            Delay -= (float)Time.Delta;
            base.Update(delta);
            return;
        }

        TimeLeft -= (float)Time.Delta;

        Progress = Math.Max(0, TimeLeft) / Duration;

        if (!Reverse) { Progress = 1 - Progress; }
        if (Easer != null) { Value = Easer(Progress); }
        else { Value = Progress; }

        OnProcess?.Invoke(this);

        if (TimeLeft <= 0) 
        { 
            TimeLeft = 0;

            switch (Mode) 
            {
                case TweenMode.Persistent:
                    Active = false;
                    break;
                case TweenMode.OneShot:
                    Active = false;
                    DetachSelf();
                    break;
                case TweenMode.Loop:
                    Start(Reverse, lastDelay);
                    break;
                case TweenMode.YoyoLoop:
                    Start(!Reverse, lastDelay);
                    break;
            }

            OnEnd?.Invoke(this);
        }
        base.Update(delta);
    }

    public void Start(float duration, bool reverse = false, float delay = 0f) 
    {  
#if DEBUG
        if (duration <= 0) { throw new Exception("Infinite Tween detected! Duration cannot be less than 0"); }
#endif
        TimeLeft = duration;
        Active = true;
        Reverse = reverse;
        TimeLeft = Duration;
        Value = Progress = Reverse ? 1 : 0;
        Delay = delay;
        lastDelay = delay;
        OnReady?.Invoke(this);
    } 

    public void Start(bool reverse = false, float delay = 0f) 
    {
        Start(Duration, reverse, delay);
    }

    public async Task WaitAsync() 
    {
        while (Active)
            await Task.Yield();
    }
}

public static class Ease 
{
    public delegate float Easer(float t);

    public static readonly Easer Linear = (float t) => t;

    public static readonly Easer SineIn = (float t) => -(float)Math.Cos(MathHelper.PiOver2 * t) + 1;
    public static readonly Easer SineOut = (float t) => (float)Math.Sin(MathHelper.PiOver2 * t);
    public static readonly Easer SineInOut = (float t) => -(float)Math.Cos(MathHelper.Pi * t) / 2f + .5f;

    public static readonly Easer QuadIn = (float t) => t * t;
    public static readonly Easer QuadOut = Invert(QuadIn);
    public static readonly Easer QuadInOut = Follow(QuadIn, QuadOut);

    public static readonly Easer CubeIn = (float t) => t * t * t;
    public static readonly Easer CubeOut = Invert(CubeIn);
    public static readonly Easer CubeInOut = Follow(CubeIn, CubeOut);

    public static readonly Easer QuintIn = (float t) => t * t * t * t * t;
    public static readonly Easer QuintOut = Invert(QuintIn);
    public static readonly Easer QuintInOut = Follow(QuintIn, QuintOut);

    public static readonly Easer ExpoIn = (float t) => (float)Math.Pow(2, 10 * (t - 1));
    public static readonly Easer ExpoOut = Invert(ExpoIn);
    public static readonly Easer ExpoInOut = Follow(ExpoIn, ExpoOut);

    public static readonly Easer BackIn = (float t) => t * t * (2.70158f * t - 1.70158f);
    public static readonly Easer BackOut = Invert(BackIn);
    public static readonly Easer BackInOut = Follow(BackIn, BackOut);

    public static readonly Easer BigBackIn = (float t) => { return t * t * (4f * t - 3f); };
    public static readonly Easer BigBackOut = Invert(BigBackIn);
    public static readonly Easer BigBackInOut = Follow(BigBackIn, BigBackOut);

    public static readonly Easer ElasticIn = (float x) => {
        float x2 = x * x;
        return x2 * x2 * (float)Math.Sin(x * MathHelper.Pi * 4.5f);
    };

    public static readonly Easer ElasticOut = (float x) => {
        float x2 = (x - 1f) * (x - 1f);
        return 1f - x2 * x2 * (float)Math.Cos(x * MathHelper.Pi * 4.5f);
    };

    public static readonly Easer ElasticInOut = (float x) => {
        float x2;
        if (x < 0.45) {
            x2 = x * x;
            return 8f * x2 * x2 * (float)Math.Sin(x * 28.27433466f);
        } else if (x < 0.55) {
            return 0.5f + 0.75f * (float)Math.Sin(x * 12.56637096f);
        } else {
            x2 = (x - 1f) * (x - 1f);
            return 1f - 8f * x2 * x2 * (float)Math.Sin(x * 28.27433466f);
        }
    };

    private const float B1 = 1f / 2.75f;
    private const float B2 = 2f / 2.75f;
    private const float B3 = 1.5f / 2.75f;
    private const float B4 = 2.5f / 2.75f;
    private const float B5 = 2.25f / 2.75f;
    private const float B6 = 2.625f / 2.75f;

    public static readonly Easer BounceIn = (float t) =>
    {
        t = 1 - t;
        return t switch 
        {
            < B1 => 1 - 7.5625f * t * t,
            < B2 => 1 - (7.5625f * (t - B3) * (t - B3) + .75f),
            < B4 => 1 - (7.5625f * (t - B5) * (t - B5) + .9375f),
            _ => 1 - (7.5625f * (t - B6) * (t - B6) + .984375f)
        };
    };

    public static readonly Easer BounceOut = (float t) =>
    {
        return t switch 
        {
            < B1 => 7.5625f * t * t,
            < B2 => 7.5625f * (t - B3) * (t - B3) + .75f,
            < B4 => 7.5625f * (t - B5) * (t - B5) + .9375f,
            _ => 7.5625f * (t - B6) * (t - B6) + .984375f
        };
    };

    public static readonly Easer BounceInOut = (float t) =>
    {
        if (t < .5f)
        {
            t = 1 - t * 2;
            return t switch 
            {
                < B1 => (1 - 7.5625f * t * t) / 2,
                < B2 => (1 - (7.5625f * (t - B3) * (t - B3) + .75f)) / 2,
                < B4 => (1 - (7.5625f * (t - B5) * (t - B5) + .9375f)) / 2,
                _ => (1 - (7.5625f * (t - B6) * (t - B6) + .984375f)) / 2
            };
        }
        t = t * 2 - 1;
        return t switch 
        {
            < B1 => (7.5625f * t * t) / 2 + .5f,
            < B2 => (7.5625f * (t - B3) * (t - B3) + .75f) / 2 + .5f,
            < B4 => (7.5625f * (t - B5) * (t - B5) + .9375f) / 2 + .5f,
            _ => (7.5625f * (t - B6) * (t - B6) + .984375f) / 2 + .5f
        };
    };

    public static Easer Invert(Easer easer)
    {
        return (float t) => { return 1 - easer(1 - t); };
    }

    public static Easer Follow(Easer first, Easer second)
    {
        return (float t) => { 
            if (t <= 0.5f) 
            {
                return first(t * 2) / 2;
            }
            return second(t * 2 - 1) / 2 + 0.5f; 
        };
    }

    public static float UpDown(float eased)
    {
        if (eased <= .5f)
            return eased * 2;
        return 1 - (eased - .5f) * 2;

    }
}