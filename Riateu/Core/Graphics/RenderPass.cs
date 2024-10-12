using System;
using SDL3;

namespace Riateu.Graphics;

public class RenderPass : IPassPool
{
    public IntPtr Handle { get; internal set; }


    public void BindGraphicsPipeline(GraphicsPipeline pipeline) 
    {
        SDL.SDL_BindGPUGraphicsPipeline(Handle, pipeline.Handle);
    }

    public unsafe void BindVertexBuffer(in BufferBinding bufferBinding, uint bindingSlot = 0) 
    {
        SDL.SDL_GPUBufferBinding gpuBufferBinding = bufferBinding.ToSDLGpu();

        SDL.SDL_BindGPUVertexBuffers(Handle, bindingSlot, [gpuBufferBinding], 1);
    }

    public unsafe void BindIndexBuffer(in BufferBinding bufferBinding, IndexElementSize elementSize) 
    {
        SDL.SDL_GPUBufferBinding gpuBufferBinding = bufferBinding.ToSDLGpu();

        SDL.SDL_BindGPUIndexBuffer(Handle, gpuBufferBinding, (SDL.SDL_GPUIndexElementSize)elementSize);
    }

    public void DrawPrimitives(uint vertexCount, uint instanceCount, uint firstVertex = 0, uint firstInstance = 0) 
    {
        SDL.SDL_DrawGPUPrimitives(Handle, vertexCount, instanceCount, firstVertex, firstInstance);
    }

    public void DrawIndexedPrimitives(uint indexCount, uint instanceCount, uint firstIndex, int vertexOffset, uint firstInstance) 
    {
        SDL.SDL_DrawGPUIndexedPrimitives(Handle, indexCount, instanceCount, firstIndex, vertexOffset, firstInstance);
    }

    public unsafe void BindVertexSampler(in TextureSamplerBinding textureSamplerBinding, uint slot = 0) 
    {
        SDL.SDL_GPUTextureSamplerBinding textureSampler = textureSamplerBinding.ToSDLGpu();

        Span<SDL.SDL_GPUTextureSamplerBinding> bindings = stackalloc SDL.SDL_GPUTextureSamplerBinding[1];
        bindings[0] = textureSampler;

        SDL.SDL_BindGPUVertexSamplers(Handle, slot, bindings, 1);
    }

    public unsafe void BindFragmentSampler(in TextureSamplerBinding textureSamplerBinding, uint slot = 0) 
    {
        SDL.SDL_GPUTextureSamplerBinding textureSampler = textureSamplerBinding.ToSDLGpu();

        Span<SDL.SDL_GPUTextureSamplerBinding> bindings = stackalloc SDL.SDL_GPUTextureSamplerBinding[1];
        bindings[0] = textureSampler;

        SDL.SDL_BindGPUFragmentSamplers(Handle, slot, bindings, 1);
    }

    public unsafe void BindFragmentSamplers(in TextureSamplerBinding firstTexture, in TextureSamplerBinding secondTexture, uint slot = 0) 
    {
        SDL.SDL_GPUTextureSamplerBinding textureSampler1 = firstTexture.ToSDLGpu();
        SDL.SDL_GPUTextureSamplerBinding textureSampler2 = secondTexture.ToSDLGpu();

        Span<SDL.SDL_GPUTextureSamplerBinding> bindings = stackalloc SDL.SDL_GPUTextureSamplerBinding[2];
        bindings[0] = textureSampler1;
        bindings[1] = textureSampler2;

        SDL.SDL_BindGPUFragmentSamplers(Handle, slot, bindings, 2);
    }

    public void SetViewport(in Viewport viewport) 
    {
        SDL.SDL_SetGPUViewport(Handle, new SDL.SDL_GPUViewport
        {
            x = viewport.X, y = viewport.Y, w = viewport.Width, h = viewport.Height
        });
    }

    public void SetScissor(in Rectangle scissor) 
    {
        SDL.SDL_SetGPUScissor(Handle, new SDL.SDL_Rect 
        {
            x = scissor.X, y = scissor.Y, w = scissor.Width, h = scissor.Height
        });
    }

    public void Obtain(IntPtr handle)
    {
        Handle = handle;
    }

    public void Reset()
    {
        Handle = IntPtr.Zero;
    }
}