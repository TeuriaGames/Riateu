using RefreshCS;

namespace Riateu.Graphics;

public class Sampler : GraphicsResource
{
    public Sampler(GraphicsDevice device, in SamplerCreateInfo info) : base(device)
    {
        Handle = Refresh.Refresh_CreateSampler(device.Handle, info.ToSDLGpu());
    }

    protected override void Dispose(bool disposing)
    {
    }

    protected override void HandleDispose(nint handle)
    {
        Refresh.Refresh_ReleaseSampler(Device.Handle, handle);
    }
}
