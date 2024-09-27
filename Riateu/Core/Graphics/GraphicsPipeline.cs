using System;
using System.Runtime.InteropServices;

namespace Riateu.Graphics;

public class GraphicsPipeline : GraphicsResource
{
    public Shader VertexShader { get; }
    public Shader FragmentShader { get; }
    public SampleCount SampleCount { get; }

    public unsafe GraphicsPipeline(GraphicsDevice device, in GraphicsPipelineCreateInfo info) : base(device)
    {
        Refresh.VertexAttribute *vertexAttributes = (Refresh.VertexAttribute*)NativeMemory.Alloc(
            (nuint)(info.VertexInputState.VertexAttributes.Length * sizeof(Refresh.VertexAttribute))
        );

        for (int i = 0; i < info.VertexInputState.VertexAttributes.Length; i++) 
        {
            vertexAttributes[i] = info.VertexInputState.VertexAttributes[i].ToSDLGpu();
        }

        Refresh.VertexBinding *vertexBindings = (Refresh.VertexBinding*)NativeMemory.Alloc(
            (nuint)(info.VertexInputState.VertexBindings.Length * sizeof(Refresh.VertexBinding))
        );

        for (int i = 0; i < info.VertexInputState.VertexBindings.Length; i++) 
        {
            vertexBindings[i] = info.VertexInputState.VertexBindings[i].ToSDLGpu();
        }

        Refresh.ColorAttachmentDescription *colorAttachmentDescriptions = stackalloc Refresh.ColorAttachmentDescription[
            info.AttachmentInfo.ColorAttachmentDescriptions.Length
        ];

        for (int i = 0; i < info.AttachmentInfo.ColorAttachmentDescriptions.Length; i++) 
        {
            colorAttachmentDescriptions[i].Format = (Refresh.TextureFormat)info.AttachmentInfo.ColorAttachmentDescriptions[i].Format;
            colorAttachmentDescriptions[i].BlendState = info.AttachmentInfo.ColorAttachmentDescriptions[i].BlendState.ToSDLGpu();
        }

        Refresh.GraphicsPipelineCreateInfo refreshGraphicsPipelineCreateInfo = new Refresh.GraphicsPipelineCreateInfo 
        {
            VertexShader = info.VertexShader.Handle,
            FragmentShader = info.FragmentShader.Handle,

            VertexInputState = new Refresh.VertexInputState() 
            {
                VertexAttributes = vertexAttributes,
                VertexAttributeCount = (uint)info.VertexInputState.VertexAttributes.Length,
                VertexBindings = vertexBindings,
                VertexBindingCount = (uint)info.VertexInputState.VertexBindings.Length
            },
            PrimitiveType = (Refresh.PrimitiveType)info.PrimitiveType,
            RasterizerState = info.RasterizerState.ToSDLGpu(),
            MultisampleState = info.MultisampleState.ToSDLGpu(),
            DepthStencilState = info.DepthStencilState.ToSDLGpu(),
            AttachmentInfo = new Refresh.GraphicsPipelineAttachmentInfo() 
            {
                ColorAttachmentCount = (uint)info.AttachmentInfo.ColorAttachmentDescriptions.Length,
                ColorAttachmentDescriptions = colorAttachmentDescriptions,
                DepthStencilFormat = (Refresh.TextureFormat)info.AttachmentInfo.DepthStencilFormat,
                HasDepthStencilAttachment = info.AttachmentInfo.HasDepthStencilAttachment ? 1 : 0
            },
        };

        refreshGraphicsPipelineCreateInfo.BlendConstants[0] = info.BlendConstants.R;
        refreshGraphicsPipelineCreateInfo.BlendConstants[1] = info.BlendConstants.G;
        refreshGraphicsPipelineCreateInfo.BlendConstants[2] = info.BlendConstants.B;
        refreshGraphicsPipelineCreateInfo.BlendConstants[3] = info.BlendConstants.A;

        Handle = Refresh.Refresh_CreateGraphicsPipeline(Device.Handle, refreshGraphicsPipelineCreateInfo);

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
        Refresh.Refresh_ReleaseGraphicsPipeline(Device.Handle, handle);
    }
}