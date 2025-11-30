using SDL3;

namespace Riateu.Graphics;

public class Sampler : GraphicsResource
{
    public Sampler(GraphicsDevice device, in SamplerCreateInfo info) : base(device)
    {
        Handle = SDL.SDL_CreateGPUSampler(device.Handle, info.ToSDLGpu());
    }

    protected override void Dispose(bool disposing)
    {
    }

    protected override void HandleDispose(nint handle)
    {
        SDL.SDL_ReleaseGPUSampler(Device.Handle, handle);
    }
}
