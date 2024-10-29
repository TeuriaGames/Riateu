using System;
using SDL3;

namespace Riateu.Graphics;

public class ComputePass : IPassPool
{
    public IntPtr Handle { get; internal set; }

    public void BindComputePipeline(ComputePipeline computePipeline) 
    {
        SDL.SDL_BindGPUComputePipeline(Handle, computePipeline.Handle);
    }

    public unsafe void BindStorageTexture(Texture texture, uint slot = 0) 
    {
        Span<nint> textureStorages = stackalloc nint[1];
        textureStorages[0] = texture.Handle;

        SDL.SDL_BindGPUComputeStorageTextures(Handle, slot, textureStorages, 1);
    }

    public unsafe void BindStorageBuffer(RawBuffer buffer, uint slot = 0) 
    {
        Span<nint> bufferStorages = stackalloc nint[1];
        bufferStorages[0] = buffer.Handle;

        SDL.SDL_BindGPUComputeStorageBuffers(Handle, slot, bufferStorages, 1);
    }

    public unsafe void BindStorageTextures(Span<Texture> textures, uint slot = 0) 
    {
        int len = textures.Length;
        Span<nint> textureStorages = stackalloc nint[len];
        for (int i = 0; i < len; i++) 
        {
            textureStorages[i] = textures[i].Handle;
        }

        SDL.SDL_BindGPUComputeStorageTextures(Handle, slot, textureStorages, (uint)len);
    }

    public unsafe void BindStorageBuffers(Span<RawBuffer> buffers, uint slot = 0) 
    {
        int len = buffers.Length;
        Span<nint> bufferStorages = stackalloc nint[len];
        for (int i = 0; i < len; i++) 
        {
            bufferStorages[i] = buffers[i].Handle;
        }

        SDL.SDL_BindGPUComputeStorageBuffers(Handle, slot, bufferStorages, (uint)len);
    }

    public void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ) 
    {
        SDL.SDL_DispatchGPUCompute(Handle, groupCountX, groupCountY, groupCountZ);
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
