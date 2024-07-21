using System;
using MoonWorks.Graphics;

namespace Riateu.Graphics;

public class RenderTarget : IDisposable
{
    private Texture texture;

    private RenderPass renderPass;
    private bool IsDisposed;

    public uint Width => texture.Width;
    public uint Height => texture.Height;

    public RenderTarget(GraphicsDevice device, uint width, uint height, uint levelCount = 1, SampleCount sampleCount = SampleCount.One) 
    {
        texture = Texture.CreateTexture2D(device, width, height, GameApp.Instance.MainWindow.SwapchainFormat, TextureUsageFlags.ColorTarget | TextureUsageFlags.Sampler, levelCount, sampleCount);
    }

    public RenderPass BeginRendering(Color clearColor) 
    {
        return GraphicsExecutor.Executor.BeginRenderPass(new ColorAttachmentInfo(texture, true, clearColor));
    }

    public void EndRendering(RenderPass renderPass) 
    {
        GraphicsExecutor.Executor.EndRenderPass(renderPass);
    }

    public static implicit operator Texture(RenderTarget target) 
    {
        return target.texture;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            texture.Dispose();
            IsDisposed = true;
        }
    }

    ~RenderTarget()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}