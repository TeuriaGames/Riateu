namespace Riateu.Graphics;

public class RenderTarget : Texture
{
    internal RenderTarget(GraphicsDevice device) : base(device) {}

    public RenderTarget(GraphicsDevice device, uint width, uint height, TextureFormat format) 
        : base(device, width, height, format, TextureUsageFlags.Sampler | TextureUsageFlags.ColorTarget)
    {
    }

    public RenderTarget(GraphicsDevice device, uint width, uint height) 
        : base(device, width, height, GameApp.Instance.MainWindow.SwapchainFormat, TextureUsageFlags.Sampler | TextureUsageFlags.ColorTarget)
    {
    }
}
