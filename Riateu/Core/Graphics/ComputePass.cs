using System;
using RefreshCS;

namespace Riateu.Graphics;

public class ComputePass : IPassPool
{
    public IntPtr Handle { get; internal set; }

    public void BindComputePipeline(ComputePipeline computePipeline) 
    {
        Refresh.Refresh_BindComputePipeline(Handle, computePipeline.Handle);
    }

    public unsafe void BindStorageTexture(TextureSlice textureSlice, uint slot = 0) 
    {
        Refresh.TextureSlice slice = textureSlice.ToSDLGpu();

        Refresh.Refresh_BindComputeStorageTextures(Handle, slot, &slice, 1);
    }

    public unsafe void BindStorageBuffer(RawBuffer buffer, uint slot = 0) 
    {
        IntPtr bufferPtr = buffer.Handle;

        Refresh.Refresh_BindComputeStorageBuffers(Handle, slot, &bufferPtr, 1);
    }

    public void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ) 
    {
        Refresh.Refresh_DispatchCompute(Handle, groupCountX, groupCountY, groupCountZ);
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
