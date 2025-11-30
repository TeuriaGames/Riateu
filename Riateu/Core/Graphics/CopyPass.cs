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
        SDL.SDL_UploadToGPUTexture(Handle, source.ToSDLGpu(), destination.ToSDLGpu(), cycle);
	}

    public void CopyTextureToTexture(in TextureLocation source, in TextureLocation destination, uint w, uint h, uint d, bool cycle) 
    {
        SDL.SDL_CopyGPUTextureToTexture(Handle, source.ToSDLGpu(), destination.ToSDLGpu(), w, h, d, cycle);
    }

    public void CopyBufferToBuffer(in BufferLocation source, in BufferLocation destination, uint size, bool cycle) 
    {
        SDL.SDL_CopyGPUBufferToBuffer(Handle, source.ToSDLGpu(), destination.ToSDLGpu(), size, cycle);
    }

    public void DownloadFromBuffer(in BufferRegion source, in TransferBufferLocation destination) 
    {
        SDL.SDL_DownloadFromGPUBuffer(Handle, source.ToSDLGpu(), destination.ToSDLGpu());
    }

    public void DownloadFromTexture(in TextureRegion source, in TextureTransferInfo destination) 
    {
        SDL.SDL_DownloadFromGPUTexture(Handle, source.ToSDLGpu(), destination.ToSDLGpu());
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