using System;
using System.Runtime.InteropServices;
using SDL3;

namespace Riateu.Graphics;

public class GraphicsPipeline : GraphicsResource
{
    public Shader VertexShader { get; }
    public Shader FragmentShader { get; }
    public SampleCount SampleCount { get; }

    public unsafe GraphicsPipeline(GraphicsDevice device, in GraphicsPipelineCreateInfo info) : base(device)
    {
        SDL.SDL_GPUVertexAttribute *vertexAttributes = (SDL.SDL_GPUVertexAttribute*)NativeMemory.Alloc(
            (nuint)(info.VertexInputState.VertexAttributes.Length * sizeof(SDL.SDL_GPUVertexAttribute))
        );

        for (int i = 0; i < info.VertexInputState.VertexAttributes.Length; i++) 
        {
            vertexAttributes[i] = info.VertexInputState.VertexAttributes[i].ToSDLGpu();
        }

        SDL.SDL_GPUVertexBufferDescription *vertexBindings = (SDL.SDL_GPUVertexBufferDescription*)NativeMemory.Alloc(
            (nuint)(info.VertexInputState.VertexBindings.Length * sizeof(SDL.SDL_GPUVertexBufferDescription))
        );

        for (int i = 0; i < info.VertexInputState.VertexBindings.Length; i++) 
        {
            vertexBindings[i] = info.VertexInputState.VertexBindings[i].ToSDLGpu();
        }

        SDL.SDL_GPUColorTargetDescription *colorAttachmentDescriptions = stackalloc SDL.SDL_GPUColorTargetDescription[
            info.AttachmentInfo.ColorAttachmentDescriptions.Length
        ];

        for (int i = 0; i < info.AttachmentInfo.ColorAttachmentDescriptions.Length; i++) 
        {
            colorAttachmentDescriptions[i].format = (SDL.SDL_GPUTextureFormat)info.AttachmentInfo.ColorAttachmentDescriptions[i].Format;
            colorAttachmentDescriptions[i].blend_state = info.AttachmentInfo.ColorAttachmentDescriptions[i].BlendState.ToSDLGpu();
        }

        SDL.SDL_GPUGraphicsPipelineCreateInfo gpuGraphicsPipelineCreateInfo = new SDL.SDL_GPUGraphicsPipelineCreateInfo 
        {
            vertex_shader = info.VertexShader.Handle,
            fragment_shader = info.FragmentShader.Handle,

            vertex_input_state = new SDL.SDL_GPUVertexInputState() 
            {
                vertex_attributes = vertexAttributes,
                num_vertex_attributes = (uint)info.VertexInputState.VertexAttributes.Length,
                vertex_buffer_descriptions = vertexBindings,
                num_vertex_buffers = (uint)info.VertexInputState.VertexBindings.Length
            },
            primitive_type = (SDL.SDL_GPUPrimitiveType)info.PrimitiveType,
            rasterizer_state = info.RasterizerState.ToSDLGpu(),
            multisample_state = info.MultisampleState.ToSDLGpu(),
            depth_stencil_state = info.DepthStencilState.ToSDLGpu(),
            target_info = new SDL.SDL_GPUGraphicsPipelineTargetInfo() 
            {
                num_color_targets = (uint)info.AttachmentInfo.ColorAttachmentDescriptions.Length,
                color_target_descriptions = colorAttachmentDescriptions,
                depth_stencil_format = (SDL.SDL_GPUTextureFormat)info.AttachmentInfo.DepthStencilFormat,
                has_depth_stencil_target = info.AttachmentInfo.HasDepthStencilAttachment
            },
        };

        Handle = SDL.SDL_CreateGPUGraphicsPipeline(Device.Handle, gpuGraphicsPipelineCreateInfo);

        if (Handle == IntPtr.Zero) 
        {
            throw new Exception("Could not create graphics pipeline.");
        }

        NativeMemory.Free(vertexBindings);
        NativeMemory.Free(vertexAttributes);

        VertexShader = info.VertexShader;
        FragmentShader = info.FragmentShader;
        SampleCount = info.MultisampleState.MultisampleCount;
    }

    protected override void Dispose(bool disposing)
    {
    }

    protected override void HandleDispose(nint handle)
    {
        SDL.SDL_ReleaseGPUGraphicsPipeline(Device.Handle, handle);
    }
}