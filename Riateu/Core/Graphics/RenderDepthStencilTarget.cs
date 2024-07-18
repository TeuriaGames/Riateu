using System;
using MoonWorks.Graphics;

namespace Riateu.Graphics;

public class RenderDepthStencilTarget : IDisposable
{
    private Texture depthTexture;
    private Texture texture;
    private bool disposedValue;

    public Texture DepthTexture => depthTexture;
    public Texture Texture => texture;

    public uint Width => texture.Width;
    public uint Height => texture.Height;

    public uint DepthStencilWidth => depthTexture.Width;
    public uint DepthStencilHeight => depthTexture.Height;
    private RenderPass renderPass;

    public RenderDepthStencilTarget(
        GraphicsDevice device, uint width, uint height, 
        TextureFormat depthStencilFormat,
        uint levelCount = 1, SampleCount sampleCount = SampleCount.One)
    {
        texture = Texture.CreateTexture2D(device, width, height, GameApp.Instance.MainWindow.SwapchainFormat, TextureUsageFlags.ColorTarget | TextureUsageFlags.Sampler, levelCount, sampleCount);
        depthTexture = Texture.CreateTexture2D(device, width, height, depthStencilFormat, TextureUsageFlags.DepthStencil, levelCount, sampleCount);
    }

    public void BeginRendering(Color clearColor) 
    {
        renderPass = GraphicsExecutor.Executor.BeginRenderPass(
            new DepthStencilAttachmentInfo(depthTexture, true, new DepthStencilValue(1, 0)),
            new ColorAttachmentInfo(texture, true, clearColor));
    }

    public void Render(IRenderable renderable) 
    {
        renderable.Render(renderPass);
    }

    public void EndRendering() 
    {
        GraphicsExecutor.Executor.EndRenderPass(renderPass);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            depthTexture.Dispose();
            disposedValue = true;
        }
    }

    ~RenderDepthStencilTarget()
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
