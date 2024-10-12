using System;
using SDL3;

namespace Riateu.Graphics;

public class Texture : GraphicsResource
{
    public uint Width { get; internal set; }
    public uint Height { get; internal set; }
    public uint LayerCountOrDepth { get; internal set; }

    public TextureFormat Format { get; internal set; }
    public TextureUsageFlags UsageFlags { get; internal set; }

    internal Texture(GraphicsDevice device) : base(device)
    {
        Width = 0;
        Height = 0;
        UsageFlags = TextureUsageFlags.ColorTarget;
    }

    public Texture(GraphicsDevice device, TextureCreateInfo info) : base(device)
    {
        var sdlGPUInfo = info.ToSDLGpu();
        Handle = SDL.SDL_CreateGPUTexture(device.Handle, sdlGPUInfo);
        Width = info.Width;
        Height = info.Height;
        LayerCountOrDepth = info.LayerCountOrDepth;

        Format = info.Format;
        UsageFlags = info.UsageFlags;
    }

    public Texture(GraphicsDevice device, uint width, uint height, TextureFormat format, TextureUsageFlags usageFlags = TextureUsageFlags.Sampler) 
        :this(device, new TextureCreateInfo 
        {
            TextureType = TextureType.Texture2D,
            Width = width,
            Height = height,
            Format = format,
            LayerCountOrDepth = 1,
            SampleCount = SampleCount.One,
            LevelCount = 1,
            UsageFlags = usageFlags
        })
    {
    }

    
    protected override void Dispose(bool disposing)
    {
    }

    public uint Download(in Span<byte> destination) 
    {
        uint size = Native.SDL_CalculateGPUTextureFormatSize((SDL.SDL_GPUTextureFormat)Format, Width, Height, LayerCountOrDepth);

#if DEBUG
        if (size > destination.Length) 
        {
            throw new Exception($"Size of a texture: '{size}' is greater than the size of a destination: '{destination.Length}'");
        }
#endif
        using TransferBuffer transferBuffer = new TransferBuffer(Device, TransferBufferUsage.Download, size);
        CommandBuffer downloadBuffer = Device.AcquireCommandBuffer();

        CopyPass pass = downloadBuffer.BeginCopyPass();
        pass.DownloadFromTexture(this, new TextureTransferInfo(transferBuffer));
        downloadBuffer.EndCopyPass(pass);

        using Fence fence = Device.SubmitAndAcquireFence(downloadBuffer);
        Device.WaitForFence(fence);

        return transferBuffer.GetTransferData(destination, 0);
    }

    public static implicit operator TextureRegion(Texture texture) 
    {
        return new TextureRegion(texture);
    }


    protected override void HandleDispose(nint handle)
    {
        SDL.SDL_ReleaseGPUTexture(Device.Handle, handle);
    }
}
