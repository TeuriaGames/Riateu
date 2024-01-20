using System.IO;
using MoonWorks;
using MoonWorks.Graphics;
using Riateu.Graphics;
using Riateu.Misc;

namespace Riateu;

public static class GameContext
{
    public static GraphicsDevice GraphicsDevice;
    public static GraphicsPipeline DefaultPipeline;
    public static Sampler GlobalSampler;

    public static void Init(GraphicsDevice device, Window mainWindow) 
    {
        GraphicsDevice = device;
        var buffer = Resources.PositionColorTexture;
        using var ms = new MemoryStream(buffer);
        ShaderModule textureShader = new ShaderModule(device, ms);

        GraphicsPipelineCreateInfo pipelineCreateInfo = new GraphicsPipelineCreateInfo() 
        {
            AttachmentInfo = new GraphicsPipelineAttachmentInfo(
                new ColorAttachmentDescription(mainWindow.SwapchainFormat, 
                ColorAttachmentBlendState.AlphaBlend)
            ),
            DepthStencilState = DepthStencilState.Disable,
            MultisampleState = MultisampleState.None,
            PrimitiveType = PrimitiveType.TriangleList,
            RasterizerState = RasterizerState.CCW_CullNone,
            VertexShaderInfo = GraphicsShaderInfo.Create<TransformVertexUniform>(textureShader, "vs_main", 0),
            FragmentShaderInfo = GraphicsShaderInfo.Create(textureShader, "fs_main", 1),
            VertexInputState = VertexInputState.CreateSingleBinding<PositionColorTextureVertex>()
        };


        DefaultPipeline = new GraphicsPipeline(device, pipelineCreateInfo);
        GlobalSampler = new Sampler(device, SamplerCreateInfo.PointClamp);
    }
}