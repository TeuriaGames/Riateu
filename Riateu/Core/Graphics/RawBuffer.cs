using SDL3;

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
                SDL.SDL_SetGPUBufferName(Device.Handle, Handle, value);
            }

            name = value;
        }
    }
    private string name = "";

    public RawBuffer(GraphicsDevice device, BufferUsageFlags usageFlags, uint sizeInBytes) : base(device)
    {
        var info = new SDL.SDL_GPUBufferCreateInfo() 
        {
            usage = (SDL.SDL_GPUBufferUsageFlags)usageFlags,
            size = sizeInBytes
        };
        Handle = SDL.SDL_CreateGPUBuffer(device.Handle, info);
        UsageFlags = usageFlags;
        Size = sizeInBytes;
    }

    protected override void Dispose(bool disposing)
    {
    }

    protected override void HandleDispose(nint handle)
    {
        SDL.SDL_ReleaseGPUBuffer(Device.Handle, handle);
    }

    public static implicit operator BufferBinding(RawBuffer buffer) 
    {
        return new BufferBinding(buffer, 0);
    }
}
   