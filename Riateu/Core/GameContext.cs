using System.IO;
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

        var vertexBufferDescription = VertexBindingAndAttributes.Create<PositionVertex>(0);
        var instancedBufferDescription = VertexBindingAndAttributes.Create<InstancedVertex>(1, 1, VertexInputRate.Instance);

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
}