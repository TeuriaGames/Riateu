using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MoonWorks.Graphics;

namespace Riateu.Graphics;

public interface IRenderable 
{
    void Render(RenderPass renderPass);
}

public class RenderQueue 
{
    private const int MAX_QUEUE = 1;
    internal IRenderable[] Queues = new IRenderable[MAX_QUEUE];
    private int queueIndex = 0;
    internal RenderQueue() {}

    public void Render(IRenderable renderable) 
    {
        if (queueIndex == Queues.Length) 
        {
            Array.Resize<IRenderable>(ref Queues, queueIndex * 2);
        }

        Queues[queueIndex++] = renderable;
    }

    internal void QueueRender(Texture backbuffer) 
    {
        CommandBuffer commandBuffer = GraphicsExecutor.Executor;

        RenderPass renderPass = commandBuffer.BeginRenderPass(new ColorAttachmentInfo(backbuffer, true, Color.Black));
        ref var start = ref MemoryMarshal.GetArrayDataReference(Queues);
        ref var end = ref Unsafe.Add(ref start, queueIndex);

        while (Unsafe.IsAddressLessThan(ref start, ref end)) 
        {
            start.Render(renderPass);
            start = ref Unsafe.Add(ref start, 1);
        }
        commandBuffer.EndRenderPass(renderPass);
        queueIndex = 0;
    }
}
