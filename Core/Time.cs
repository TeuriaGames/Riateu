using System;

namespace Riateu;

public static class Time 
{
    public static double DeltaScale = 1.0;
    public static double Delta { get; internal set; }
    public static double FPS { get; internal set; }

    private static int fpsCounter;
    private static TimeSpan counterElapsed;

    public static void Update(in TimeSpan delta) 
    {
        Delta = delta.TotalSeconds * DeltaScale;
        fpsCounter++;
        if (counterElapsed < TimeSpan.FromSeconds(1)) return;
        counterElapsed += delta;
        FPS = fpsCounter;
        counterElapsed -= TimeSpan.FromSeconds(1);
    }
}