namespace Riateu.Graphics;

public class DepthTarget : Texture
{
    internal DepthTarget(GraphicsDevice device) : base(device) {}

    public DepthTarget(GraphicsDevice device, uint width, uint height, TextureFormat format, uint depth = 1) 
        : base(device, width, height, format, TextureUsageFlags.DepthStencil)
    {
    }

    public DepthTarget(GraphicsDevice device, uint width, uint height, uint depth = 1) 
        : base(device, width, height, TextureFormat.D24_UNORM, TextureUsageFlags.DepthStencil)
    {
    }
}