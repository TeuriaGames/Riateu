using System;
using RefreshCS;

namespace Riateu.Graphics;

public class Fence : GraphicsResource, IGraphicsPool
{
    public Fence() : base(null) {}

    public void Obtain(GraphicsDevice device)
    {
        Reinit(device);
    }

    public void Reset()
    {
        Handle = IntPtr.Zero;
    }

    protected override void Dispose(bool disposing)
    {
    }

    protected override void HandleDispose(nint handle)
    {
        Refresh.Refresh_ReleaseFence(Device.Handle, handle);
    }
}