using MoonWorks.Graphics;

namespace Riateu.Graphics;

public static class GraphicsExecutor
{
    public static CommandBuffer Executor => executor;
    private static CommandBuffer executor;


    internal static CommandBuffer Acquire(GraphicsDevice device) 
    {
        executor = device.AcquireCommandBuffer();
        return executor;
    }

    internal static void Submit(GraphicsDevice device) 
    {
        device.Submit(Executor);
        executor = null;
    }
}