using System.IO;
using System.Runtime.InteropServices;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Components;
using Riateu.Graphics;
using Riateu.Misc;

namespace Riateu;

public static class GameContext
{
    public static GraphicsDevice GraphicsDevice;
    public static GraphicsPipeline DefaultPipeline;
    public static GraphicsPipeline MSDFPipeline;
    public static GraphicsPipeline InstancedPipeline;
    public static Sampler GlobalSampler;

    public static void Init(GraphicsDevice device, Window mainWindow) 
    {
        GraphicsDevice = device;
        GlobalSampler = new Sampler(device, SamplerCreateInfo.PointClamp);
        var positionTextureColor = Resources.PositionTextureColor;
        using var ms1 = new MemoryStream(positionTextureColor);
        ShaderModule vertexPSC = new ShaderModule(device, ms1);

        var textureFragment = Resources.Texture;
        using var ms2 = new MemoryStream(textureFragment);
        ShaderModule fragmentPSC = new ShaderModule(device, ms2);

        GraphicsPipelineCreateInfo pipelineCreateInfo = new GraphicsPipelineCreateInfo() 
        {
            AttachmentInfo = new GraphicsPipelineAttachmentInfo(
                new ColorAttachmentDescription(mainWindow.SwapchainFormat, 
                ColorAttachmentBlendState.AlphaBlend)
            ),
            DepthStencilState = DepthStencilState.Disable,
            MultisampleState = MultisampleState.None,
            PrimitiveType = PrimitiveType.TriangleList,
            RasterizerState = RasterizerState.CW_CullNone,
            VertexShaderInfo = GraphicsShaderInfo.Create<Matrix4x4>(vertexPSC, "vs_main", 0),
            FragmentShaderInfo = GraphicsShaderInfo.Create(fragmentPSC, "fs_main", 1),
            VertexInputState = VertexInputState.CreateSingleBinding<PositionTextureColorVertex>()
        };

        DefaultPipeline = new GraphicsPipeline(device, pipelineCreateInfo);

        GraphicsPipelineCreateInfo msdfPipelineCreateInfo = new GraphicsPipelineCreateInfo() 
        {
            AttachmentInfo = new GraphicsPipelineAttachmentInfo(
                new ColorAttachmentDescription(mainWindow.SwapchainFormat, 
                ColorAttachmentBlendState.AlphaBlend)
            ),
            DepthStencilState = DepthStencilState.Disable,
            MultisampleState = MultisampleState.None,
            PrimitiveType = PrimitiveType.TriangleList,
            RasterizerState = RasterizerState.CW_CullNone,
            VertexShaderInfo = device.TextVertexShaderInfo,
            FragmentShaderInfo = device.TextFragmentShaderInfo,
            VertexInputState = device.TextVertexInputState
        };

        MSDFPipeline = new GraphicsPipeline(device, msdfPipelineCreateInfo);


        var vertexBufferDescription = new VertexBindingAndAttributes(
            new VertexBinding 
            {
                Binding = 0,
                InputRate = VertexInputRate.Vertex,
                Stride = (uint)Marshal.SizeOf<PositionVertex>()
            },
            CreateVertexAttribute<PositionVertex>(0)
        );

        var instancedBufferDescription = new VertexBindingAndAttributes(
            new VertexBinding 
            {
                Binding = 1,
                InputRate = VertexInputRate.Instance,
                Stride = (uint)Marshal.SizeOf<InstancedVertex>()
            },
            CreateVertexAttribute<InstancedVertex>(1, 1)
        );

        var tileMapBytes = Resources.InstancedShader;
        using var ms3 = new MemoryStream(tileMapBytes);
        ShaderModule instancedPSC = new ShaderModule(device, ms3);

        GraphicsPipelineCreateInfo instancedPipelineCreateInfo = new GraphicsPipelineCreateInfo() 
        {
            AttachmentInfo = new GraphicsPipelineAttachmentInfo(
                new ColorAttachmentDescription(mainWindow.SwapchainFormat, 
                ColorAttachmentBlendState.AlphaBlend)
            ),
            DepthStencilState = DepthStencilState.Disable,
            MultisampleState = MultisampleState.None,
            PrimitiveType = PrimitiveType.TriangleList,
            RasterizerState = RasterizerState.CW_CullNone,
            VertexShaderInfo = GraphicsShaderInfo.Create<Matrix4x4>(instancedPSC, "vs_main", 0),
            FragmentShaderInfo = GraphicsShaderInfo.Create(instancedPSC, "fs_main", 1),
            VertexInputState = new VertexInputState([
                vertexBufferDescription,
                instancedBufferDescription
            ])
        };

        InstancedPipeline = new GraphicsPipeline(device, instancedPipelineCreateInfo);
    }

    public static VertexAttribute[] CreateVertexAttribute<T>(
        uint bindingIndex, uint startingLocation = 0, uint offsetStart = 0) 
        where T : unmanaged, IVertexType
    {
        VertexAttribute[] attributes = new VertexAttribute[T.Formats.Length];
        uint offset = offsetStart;

        for (uint i = 0; i < T.Formats.Length; i += 1)
        {
            var format = T.Formats[i];

            attributes[i] = new VertexAttribute
            {
                Binding = bindingIndex,
                Location = i + startingLocation,
                Format = format,
                Offset = offset
            };

            offset += Conversions.VertexElementFormatSize(format);
        }
        return attributes;
    }
}