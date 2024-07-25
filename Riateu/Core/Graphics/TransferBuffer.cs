using System;
using RefreshCS;

namespace Riateu.Graphics;


public class TransferBuffer : GraphicsResource
{
    public uint Size { get; }

#if DEBUG
    public bool IsMapped { get; private set; }
#endif

    public TransferBuffer(GraphicsDevice device, TransferBufferUsage usage, uint size) : base(device)
    {
        Handle = Refresh.Refresh_CreateTransferBuffer(Device.Handle, (Refresh.TransferBufferUsage)usage, size);
        Size = size;
    }

    public unsafe static TransferBuffer Create<T>(GraphicsDevice device, TransferBufferUsage usage, uint size) 
    where T : unmanaged
    {
        return new TransferBuffer(device, usage, size * (uint)sizeof(T));
    }

    public unsafe void Map(bool cycle, out byte *data) 
    {
#if DEBUG
        AssertNotMapped();
#endif
        IsMapped = true;
        Refresh.Refresh_MapTransferBuffer(Device.Handle, Handle, cycle ? 1 : 0, out data);
    }

    public void Unmap() 
    {
        IsMapped = false;
        Refresh.Refresh_UnmapTransferBuffer(Device.Handle, Handle);
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

        fixed (T* dataPtr = source) 
        {
            Refresh.Refresh_SetTransferData(Device.Handle, (nint)dataPtr, new Refresh.TransferBufferRegion() 
            {
                TransferBuffer = Handle,
                Offset = bufferOffsetInBytes,
                Size = dataLengthInBytes
            }, cycle ? 1 : 0);
        }

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

        fixed (T* dataPtr = dest) 
        {
            Refresh.Refresh_GetTransferData(Device.Handle, new Refresh.TransferBufferRegion() 
            {
                TransferBuffer = Handle,
                Offset = bufferOffsetInBytes,
                Size = dataLengthInBytes
            }, (nint)dataPtr);
        }

        return dataLengthInBytes;
    }


    protected override void Dispose(bool disposing)
    {
    }

    protected override void HandleDispose(nint handle)
    {
        Refresh.Refresh_ReleaseTransferBuffer(Device.Handle, handle);
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