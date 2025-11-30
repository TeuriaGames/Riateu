using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL3;

namespace Riateu.Graphics;


public class TransferBuffer : GraphicsResource
{
    public uint Size { get; }
    private IntPtr mappedPointer;


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

    public void Map(bool cycle) 
    {
        if (mappedPointer != IntPtr.Zero) 
        {
            return;
        }
        mappedPointer = (IntPtr)SDL.SDL_MapGPUTransferBuffer(Device.Handle, Handle, cycle);
    }

    public unsafe Span<byte> Map(bool cycle, uint offset = 0) 
    {
#if DEBUG
        AssertNotMapped();
#endif
        Map(cycle);
        return new Span<byte>((void*)(mappedPointer + offset), (int)(Size - offset));
    }

    public unsafe Span<T> Map<T>(bool cycle, uint offset = 0) 
    where T : unmanaged
    {
#if DEBUG
        AssertNotMapped();
#endif
        Map(cycle);
        return new Span<T>((void*)(mappedPointer + offset), (int)(Size - offset) / Unsafe.SizeOf<T>());
    }

    public unsafe Span<T> MappedTouch<T>(uint offset = 0) 
    {
#if DEBUG
        AssertMapped();
#endif
        return new Span<T>((void*)(mappedPointer + offset), (int)(Size - offset) / Unsafe.SizeOf<T>());
    }

    public void Unmap() 
    {
        SDL.SDL_UnmapGPUTransferBuffer(Device.Handle, Handle);
        mappedPointer = IntPtr.Zero;
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
    private void AssertMapped() 
    {
        if (mappedPointer == IntPtr.Zero) 
        {
            throw new Exception("Transfer buffer has not mapped yet.");
        }
    }

    private void AssertNotMapped() 
    {
        if (mappedPointer != IntPtr.Zero) 
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