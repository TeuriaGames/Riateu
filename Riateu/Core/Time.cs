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
    private static Stopwatch sw = Stopwatch.StartNew();

    /// <summary>
    /// This method sync from the update loop.
    /// </summary>
    public static void Update(in TimeSpan delta) 
    {
        Delta = delta.TotalSeconds * DeltaScale;
    }

    public static void Draw(double alpha) 
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