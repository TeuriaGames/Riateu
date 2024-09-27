using System;
using SDL3;

namespace Riateu.Graphics;

public class CopyPass : IPassPool
{
    public IntPtr Handle { get; internal set; }

    public void UploadToBuffer(in TransferBufferLocation source, in BufferRegion destination, bool cycle) 
    {
        SDL.SDL_UploadToGPUBuffer(Handle, source.ToSDLGpu(), destination.ToSDLGpu(), cycle);
    }

    public void UploadToBuffer(TransferBuffer source, RawBuffer destination, bool cycle) 
    {
        UploadToBuffer(
            new TransferBufferLocation(source),
            new BufferRegion(destination, 0, destination.Size),
            cycle
        );
    }

	public void UploadToTexture(TransferBuffer source, Texture destination, bool cycle) 
    {
		UploadToTexture(new TextureTransferInfo(source), new TextureRegion(destination), cycle);
	}

	public void UploadToTexture(in TextureTransferInfo source, in TextureRegion destination, bool cycle) 
    {
		Refresh.Refresh_UploadToTexture(Handle, source.ToSDLGpu(), destination.ToSDLGpu(), cycle ? 1 : 0);
	}

    public void CopyTextureToTexture(in TextureLocation source, in TextureLocation destination, uint w, uint h, uint d, bool cycle) 
    {
        Refresh.Refresh_CopyTextureToTexture(Handle, source.ToSDLGpu(), destination.ToSDLGpu(), w, h, d, cycle ? 1 : 0);
    }

    public void CopyBufferToBuffer(in BufferLocation source, in BufferLocation destination, uint size, bool cycle) 
    {
        Refresh.Refresh_CopyBufferToBuffer(Handle, source.ToSDLGpu(), destination.ToSDLGpu(), size, cycle ? 1 : 0);
    }

    public void DownloadFromBuffer(in BufferRegion source, in TransferBufferLocation destination) 
    {
        Refresh.Refresh_DownloadFromBuffer(Handle, source.ToSDLGpu(), destination.ToSDLGpu());
    }

    public void DownloadFromTexture(in TextureRegion source, in TextureTransferInfo destination) 
    {
        Refresh.Refresh_DownloadFromTexture(Handle, source.ToSDLGpu(), destination.ToSDLGpu());
    }

    public void Obtain(nint handle)
    {
        Handle = handle;
    }

    public void Reset()
    {
        Handle = IntPtr.Zero;
    }
}