using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Riateu;

public sealed class Shaker : Component 
{
    [InlineArray(5)]
    private struct ShakeOffset 
    {
        private int _element0;
    }
    public float Timer { get; private set; }
    public Vector2 Value { get; private set; }
    public float Intensity { get; set; }

    private static Random random = new Random();
    private static ShakeOffset offsets = new ShakeOffset();

    public Shaker() 
    {
        Active = false;
        offsets[0] = -1;
        offsets[1] = -1;
        offsets[2] = 0;
        offsets[3] = 1;
        offsets[4] = 1;
    }


    public override void Update(double delta)
    {
        if (!Active && Timer <= 0) 
        {
            return;
        }
        Timer -= (float)Time.Delta;
        if (Timer <= 0) 
        {
            Active = false;
            Value = Vector2.Zero;
            return;
        }
        Value = new Vector2(offsets[random.Next(5)], offsets[random.Next(5)]) * Intensity;

        base.Update(delta);
    }

    public void ShakeFor(float timer) 
    {
        Timer = timer;
        Active = true;
    }
}