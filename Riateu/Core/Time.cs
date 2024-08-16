using System;
using System.Diagnostics;

namespace Riateu;

/// <summary>
/// Contains a update loop properties.
/// </summary>
public static class Time 
{
    /// <summary>
    /// An update rate for the delta time. Commonly used for time speed.
    /// </summary>
    public static float DeltaScale = 1.0f;
    /// <summary>
    /// A completion of time since the last frame.
    /// </summary>
    public static float RawDelta { get; internal set; }
    /// <summary>
    /// A completion of time since the last frame within the update rate.
    /// </summary>
    public static float Delta { get; internal set; }
    /// <summary>
    /// The frames per second of the update loop
    /// </summary>
    public static double FPS { get; internal set; }
    public static int Frame { get; internal set; }
    public static TimeSpan Duration { get; internal set; }

    private static int fpsCounter;
    private static Stopwatch sw = Stopwatch.StartNew();

    public static void Draw() 
    {
        fpsCounter++;
        var elapsed = sw.Elapsed.TotalSeconds;
        if (elapsed > 1) 
        {
            sw.Restart();
            FPS = fpsCounter;
            fpsCounter = 0;
        }
    }
}