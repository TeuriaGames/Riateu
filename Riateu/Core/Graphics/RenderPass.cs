using System;
using RefreshCS;

namespace Riateu.Graphics;

public class RenderPass : IPassPool
{
    public IntPtr Handle { get; internal set; }


    public void BindGraphicsPipeline(GraphicsPipeline pipeline) 
    {
        Refresh.Refresh_BindGraphicsPipeline(Handle, pipeline.Handle);
    }

    public unsafe void BindVertexBuffer(in BufferBinding bufferBinding, uint bindingSlot = 0) 
    {
        Refresh.BufferBinding gpuBufferBinding = bufferBinding.ToSDLGpu();

        Refresh.Refresh_BindVertexBuffers(Handle, bindingSlot, &gpuBufferBinding, 1);
    }

    public unsafe void BindIndexBuffer(in BufferBinding bufferBinding, IndexElementSize elementSize) 
    {
        Refresh.BufferBinding gpuBufferBinding = bufferBinding.ToSDLGpu();

        Refresh.Refresh_BindIndexBuffer(Handle, gpuBufferBinding, (Refresh.IndexElementSize)elementSize);
    }

    public void DrawPrimitives(uint vertexStart, uint primitiveCount) 
    {
        Refresh.Refresh_DrawPrimitives(Handle, vertexStart, primitiveCount);
    }

    public void DrawIndexedPrimitives(uint baseVertex, uint startIndex, uint primitiveCount, uint instanceCount = 1) 
    {
        Refresh.Refresh_DrawIndexedPrimitives(Handle, baseVertex, startIndex, primitiveCount, instanceCount);
    }

    public unsafe void BindVertexSampler(in TextureSamplerBinding textureSamplerBinding, uint slot = 0) 
    {
        Refresh.TextureSamplerBinding textureSampler = textureSamplerBinding.ToSDLGpu();

        Refresh.Refresh_BindVertexSamplers(Handle, slot, &textureSampler, 1);
    }

    public unsafe void BindFragmentSampler(in TextureSamplerBinding textureSamplerBinding, uint firstSlot = 0) 
    {
        Refresh.TextureSamplerBinding bind = textureSamplerBinding.ToSDLGpu();
        Refresh.Refresh_BindFragmentSamplers(Handle, firstSlot, &bind, 1);
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