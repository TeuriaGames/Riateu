using System;
using RefreshCS;

namespace Riateu.Graphics;

public class Texture : GraphicsResource
{
    public uint Width { get; internal set; }
    public uint Height { get; internal set; }
    public uint Depth { get; internal set; }

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
        Handle = Refresh.Refresh_CreateTexture(device.Handle, info.ToSDLGpu());
        Width = info.Width;
        Height = info.Height;
        Depth = info.Depth;

        Format = info.Format;
        UsageFlags = info.UsageFlags;
    }

    public Texture(GraphicsDevice device, uint width, uint height, TextureFormat format, TextureUsageFlags usageFlags = TextureUsageFlags.Sampler) 
        :this(device, new TextureCreateInfo 
        {
            Width = width,
            Height = height,
            Format = format,
            IsCube = false,
            Depth = 1,
            LayerCount = 1,
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
        uint size = Width * Height * BytesPerPixel(Format);

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

    public static implicit operator TextureSlice(Texture texture) 
    {
        return new TextureSlice(texture);
    }

    public static uint BytesPerPixel(TextureFormat format)
    {
        switch (format)
        {
            case TextureFormat.R8:
            case TextureFormat.A8:
            case TextureFormat.R8_UINT:
                return 1;
            case TextureFormat.B5G6R5:
            case TextureFormat.B4G4R4A4:
            case TextureFormat.B5G5R5A1:
            case TextureFormat.R16_SFLOAT:
            case TextureFormat.R8G8_SNORM:
            case TextureFormat.R8G8_UINT:
            case TextureFormat.R16_UINT:
            case TextureFormat.D16_UNORM:
                return 2;
            case TextureFormat.R8G8B8A8:
            case TextureFormat.B8G8R8A8:
            case TextureFormat.R32_SFLOAT:
            case TextureFormat.R16G16:
            case TextureFormat.R16G16_SFLOAT:
            case TextureFormat.R8G8B8A8_SNORM:
            case TextureFormat.R10G10B10A2:
            case TextureFormat.R8G8B8A8_UINT:
            case TextureFormat.R16G16_UINT:
            case TextureFormat.D24_UNORM_S8_UINT:
            case TextureFormat.D32_SFLOAT:
                return 4;
            case TextureFormat.D32_SFLOAT_S8_UINT:
                return 5;
            case TextureFormat.R16G16B16A16_SFLOAT:
            case TextureFormat.R16G16B16A16:
            case TextureFormat.R32G32_SFLOAT:
            case TextureFormat.R16G16B16A16_UINT:
            case TextureFormat.BC1:
                return 8;
            case TextureFormat.R32G32B32A32_SFLOAT:
            case TextureFormat.BC2:
            case TextureFormat.BC3:
            case TextureFormat.BC7:
                return 16;
            default:
                return 0;
        }
    }

    protected override void HandleDispose(nint handle)
    {
        Refresh.Refresh_ReleaseTexture(Device.Handle, handle);
    }
}
