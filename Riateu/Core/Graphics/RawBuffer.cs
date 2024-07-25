using RefreshCS;

namespace Riateu.Graphics;

public class RawBuffer : GraphicsResource
{
    public BufferUsageFlags UsageFlags { get; }
    public uint Size { get; }

    public string Name 
    {
        get => name;
        set 
        {
            if (Device.DebugMode) 
            {
                Refresh.Refresh_SetBufferName(Device.Handle, Handle, value);
            }

            name = value;
        }
    }
    private string name = "";

    public RawBuffer(GraphicsDevice device, BufferUsageFlags usageFlags, uint sizeInBytes) : base(device)
    {
        Handle = Refresh.Refresh_CreateBuffer(device.Handle, (Refresh.BufferUsageFlags)usageFlags, sizeInBytes);
        UsageFlags = usageFlags;
        Size = sizeInBytes;
    }

    protected override void Dispose(bool disposing)
    {
    }

    protected override void HandleDispose(nint handle)
    {
        Refresh.Refresh_ReleaseBuffer(Device.Handle, handle);
    }

    public static implicit operator BufferBinding(RawBuffer buffer) 
    {
        return new BufferBinding(buffer, 0);
    }
}
   