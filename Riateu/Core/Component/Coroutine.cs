using System;
using System.Threading;
using System.Threading.Tasks;
using Riateu.Misc;

namespace Riateu.Components;

/// <summary>
/// A component that allows an async-method to run in sequence per frame, utilizing the 
/// await syntax. It uses a custom <see cref="System.Threading.SynchronizationContext"/>
/// to prevent multiple threads while running an async-coroutine method.
/// </summary>
public class Coroutine : Component
{
    private CoroutineContext scheduler = new();


    private async Task WrapCoroutine(Func<Task> coroutine)
    {
        try 
        {
            await Task.Yield();
            await coroutine();
        }
        catch (Exception e) 
        {
            Console.WriteLine(e.ToString());
            throw;
        }
    }

    /// <summary>
    /// A method to run a async-coroutine methods.
    /// </summary>
    /// <param name="coroutine">An async method to run</param>
    /// <returns>An awaitable unit <see cref="System.Threading.Tasks.Task"/></returns>
    public Task Run(Func<Task> coroutine) 
    {
        var oldContext = SynchronizationContext.Current;
        try 
        {
            var syncContext = (SynchronizationContext)scheduler;
            SynchronizationContext.SetSynchronizationContext(syncContext);
            var task = WrapCoroutine(coroutine);
            return task;
        }
        finally 
        {
            SynchronizationContext.SetSynchronizationContext(oldContext);
        }
    }

    /// <inheritdoc/>
    public override void Update(double delta)
    {
        scheduler.Update();
    }

    /// <summary>
    /// A method to check if the coroutine is currently running.
    /// </summary>
    /// <returns>A boolean value that checks if the coroutine is running or not</returns>
    public bool IsRunning() => scheduler.IsRunning;
    
    /// <summary>
    /// An async-coroutine method that waits for seconds per frame.
    /// </summary>
    /// <param name="seconds">An amount of seconds to wait</param>
    /// <returns>An awaitable unit <see cref="System.Threading.Tasks.ValueTask"/></returns>    
    public static async ValueTask Wait(double seconds) 
    {
        var timer = new WaitTimer();
        await timer.WaitFor(seconds);
    }
}

internal struct WaitTimer 
{
    private double timer;

    internal async ValueTask WaitFor(double seconds) 
    {
        timer = seconds;
        while (timer > 0) 
        {
            timer -= Time.Delta;
            await Task.Yield();
        }
    }
}