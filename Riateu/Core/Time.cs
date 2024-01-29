using System;

namespace Riateu;

/// <summary>
/// Contains a update loop properties.
/// </summary>
public static class Time 
{
    /// <summary>
    /// An update rate for the delta time. Commonly used for time speed.
    /// </summary>
    public static double DeltaScale = 1.0;
    /// <summary>
    /// A completion of time since the last frame.
    /// </summary>
    public static double Delta { get; internal set; }
    /// <summary>
    /// The frames per second of the update loop
    /// </summary>
    public static double FPS { get; internal set; }

    private static int fpsCounter;
    private static TimeSpan counterElapsed;

    /// <summary>
    /// This method sync the from the update loop.
    /// </summary>
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