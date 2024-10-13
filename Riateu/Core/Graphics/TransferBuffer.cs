using System;
using System.Runtime.InteropServices;
using SDL3;

namespace Riateu.Graphics;


public class TransferBuffer : GraphicsResource
{
    public uint Size { get; }

#if DEBUG
    public bool IsMapped { get; private set; }
#endif

    public TransferBuffer(GraphicsDevice device, TransferBufferUsage usage, uint size) : base(device)
    {
        var info = new SDL.SDL_GPUTransferBufferCreateInfo() {
            usage = (SDL.SDL_GPUTransferBufferUsage)usage,
            size = size
        };
        Handle = SDL.SDL_CreateGPUTransferBuffer(Device.Handle, info);
        Size = size;
    }

    public unsafe static TransferBuffer Create<T>(GraphicsDevice device, TransferBufferUsage usage, uint size) 
    where T : unmanaged
    {
        return new TransferBuffer(device, usage, size * (uint)sizeof(T));
    }

    public unsafe byte* UnsafeMap(bool cycle) 
    {
#if DEBUG
        AssertNotMapped();
        IsMapped = true;
#endif
        var data = (byte*)SDL.SDL_MapGPUTransferBuffer(Device.Handle, Handle, cycle);
        return data;
    }

    public unsafe Span<byte> Map(bool cycle) 
    {
#if DEBUG
        AssertNotMapped();
        IsMapped = true;
#endif
        var data = (byte*)SDL.SDL_MapGPUTransferBuffer(Device.Handle, Handle, cycle);
        return new Span<byte>(data, (int)Size);
    }

    public unsafe Span<T> Map<T>(bool cycle) 
    {
#if DEBUG
        AssertNotMapped();
        IsMapped = true;
#endif
        var data = (byte*)SDL.SDL_MapGPUTransferBuffer(Device.Handle, Handle, cycle);
        return new Span<T>(data, (int)Size);
    }

    public void Unmap() 
    {
#if DEBUG
        IsMapped = false;
#endif
        SDL.SDL_UnmapGPUTransferBuffer(Device.Handle, Handle);
    }

    public unsafe uint SetTransferData<T>(Span<T> source, uint bufferOffsetInBytes, bool cycle) 
    where T : unmanaged
    {
        int elementSize = sizeof(T);
        uint dataLengthInBytes = (uint)(elementSize * source.Length);
#if DEBUG
        if (dataLengthInBytes > Size + bufferOffsetInBytes) 
        {
            throw new InvalidOperationException($"Data overflow! Transfer buffer length {Size}, offset {bufferOffsetInBytes}, copy length {dataLengthInBytes}");
        }
        AssertNotMapped();
#endif

        byte *mappedBuffer = (byte*)SDL.SDL_MapGPUTransferBuffer(Device.Handle, Handle, cycle);
        fixed (T* dataPtr = source) 
        {
            NativeMemory.Copy(dataPtr, mappedBuffer, dataLengthInBytes + bufferOffsetInBytes);
        }
        SDL.SDL_UnmapGPUTransferBuffer(Device.Handle, Handle);

        return dataLengthInBytes;
    }

    public unsafe uint GetTransferData<T>(Span<T> dest, uint bufferOffsetInBytes) 
    where T : unmanaged
    {
        int elementSize = sizeof(T);
        uint dataLengthInBytes = (uint)(elementSize * dest.Length);
#if DEBUG
        if (dataLengthInBytes > Size + bufferOffsetInBytes) 
        {
            throw new InvalidOperationException($"Data overflow! Transfer buffer length {Size}, offset {bufferOffsetInBytes}, copy length {dataLengthInBytes}");
        }
        AssertNotMapped();
#endif

        byte *mappedBuffer = (byte*)SDL.SDL_MapGPUTransferBuffer(Device.Handle, Handle, false);
        fixed (T *dataPtr = dest) 
        {
            NativeMemory.Copy(mappedBuffer, &dataPtr[bufferOffsetInBytes], dataLengthInBytes);
        }
        SDL.SDL_UnmapGPUTransferBuffer(Device.Handle, Handle);

        return dataLengthInBytes;
    }


    protected override void Dispose(bool disposing)
    {
    }

    protected override void HandleDispose(nint handle)
    {
        SDL.SDL_ReleaseGPUTransferBuffer(Device.Handle, handle);
    }

#if DEBUG
    private void AssertNotMapped() 
    {
        if (IsMapped) 
        {
            throw new Exception("Transfer buffer is still mapping.");
        }
    }
#endif
}
   
public interface IVertexFormat 
{
    abstract static VertexAttribute[] Attributes(uint binding);
}