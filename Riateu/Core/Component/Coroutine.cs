using System;
using System.Threading;
using System.Threading.Tasks;
using Riateu.Misc;

namespace Riateu.Components;

public class Coroutine : Component
{
    private CoroutineContext scheduler = new();


    private async Task WrapCoroutine(Func<Task> coroutine)
    {
        await Task.Yield();
        await coroutine();
    }

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

    public override void Update(double delta)
    {
        scheduler.Update();
    }

    public bool IsRunning() => scheduler.IsRunning;
    

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