using System.IO;
using MoonWorks;
using MoonWorks.Graphics;
using Riateu.Graphics;
using Riateu.Misc;

namespace Riateu;

/// <summary>
/// A class that contains all of the default pipelines and features that are needed for
/// other renderers like texts or standard rendering stuffs.
/// </summary>
public static class GameContext
{
    /// <summary>
    /// The application graphics device.
    /// </summary>
    public static GraphicsDevice GraphicsDevice;
    /// <summary>
    /// The default rendering pipeline use for basic rendering.
    /// </summary>
    public static GraphicsPipeline DefaultPipeline;
    public static GraphicsPipeline RGBPipeline;
    /// <summary>
    /// A rendering pipeline designed specifically for text rendered in msdf format.
    /// </summary>
    public static GraphicsPipeline MSDFPipeline;
    /// <summary>
    /// An instanced rendering pipeline use as an fast alternative for default pipeline.
    /// </summary>
    public static GraphicsPipeline InstancedPipeline;
    /// <summary>
    /// An everything sampler that uses point clamp for sampling.
    /// </summary>
    public static Sampler GlobalSampler;
    internal static DrawBatchPool DrawBatchPool = new DrawBatchPool();

    internal static void Init(GraphicsDevice device, Window mainWindow)
    {
        GraphicsDevice = device;
        GlobalSampler = new Sampler(device, SamplerCreateInfo.PointClamp);
        var positionTextureColor = Resources.PositionTextureColor;
        using var ms1 = new MemoryStream(positionTextureColor);
        Shader vertexPSC = new Shader(device, ms1, "main", new ShaderCreateInfo {
            ShaderStage = ShaderStage.Vertex,
            ShaderFormat = ShaderFormat.SPIRV,
            UniformBufferCount = 1
        });

        var textureFragment = Resources.Texture;
        using var ms2 = new MemoryStream(textureFragment);
        Shader fragmentPSC = new Shader(device, ms2, "main", new ShaderCreateInfo {
            ShaderStage = ShaderStage.Fragment,
            ShaderFormat = ShaderFormat.SPIRV,
            SamplerCount = 1
        });

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
            VertexShader = vertexPSC,
            FragmentShader = fragmentPSC,
            VertexInputState = VertexInputState.CreateSingleBinding<PositionTextureColorVertex>()
        };

        DefaultPipeline = new GraphicsPipeline(device, pipelineCreateInfo);

        GraphicsPipelineCreateInfo rgbCreateInfo = new GraphicsPipelineCreateInfo()
        {
            AttachmentInfo = new GraphicsPipelineAttachmentInfo(
                new ColorAttachmentDescription(TextureFormat.R8G8B8A8,
                ColorAttachmentBlendState.AlphaBlend)
            ),
            DepthStencilState = DepthStencilState.Disable,
            MultisampleState = MultisampleState.None,
            PrimitiveType = PrimitiveType.TriangleList,
            RasterizerState = RasterizerState.CCW_CullNone,
            VertexShader = vertexPSC,
            FragmentShader = fragmentPSC,
            VertexInputState = VertexInputState.CreateSingleBinding<PositionTextureColorVertex>()
        };

        RGBPipeline = new GraphicsPipeline(device, rgbCreateInfo);

        GraphicsPipelineCreateInfo msdfPipelineCreateInfo = new GraphicsPipelineCreateInfo()
        {
            AttachmentInfo = new GraphicsPipelineAttachmentInfo(
                new ColorAttachmentDescription(TextureFormat.R8G8B8A8,
                ColorAttachmentBlendState.AlphaBlend)
            ),
            DepthStencilState = DepthStencilState.Disable,
            MultisampleState = MultisampleState.None,
            PrimitiveType = PrimitiveType.TriangleList,
            RasterizerState = RasterizerState.CCW_CullNone,
            VertexShader = device.TextVertexShader,
            FragmentShader = device.TextFragmentShader,
            VertexInputState = device.TextVertexInputState
        };

        MSDFPipeline = new GraphicsPipeline(device, msdfPipelineCreateInfo);

        var vertexBufferDescription = VertexBindingAndAttributes.Create<PositionVertex>(0);
        var instancedBufferDescription = VertexBindingAndAttributes.Create<InstancedVertex>(1, 1, VertexInputRate.Instance);

        var tileMapBytes = Resources.InstancedShader;
        using var ms3 = new MemoryStream(tileMapBytes);
        Shader instancedPSC = new Shader(device, ms3, "main", new ShaderCreateInfo {
            ShaderStage = ShaderStage.Vertex,
            ShaderFormat = ShaderFormat.SPIRV,
            UniformBufferCount = 1
        });

        GraphicsPipelineCreateInfo instancedPipelineCreateInfo = new GraphicsPipelineCreateInfo()
        {
            AttachmentInfo = new GraphicsPipelineAttachmentInfo(
                new ColorAttachmentDescription(TextureFormat.R8G8B8A8,
                ColorAttachmentBlendState.AlphaBlend)
            ),
            DepthStencilState = DepthStencilState.Disable,
            MultisampleState = MultisampleState.None,
            PrimitiveType = PrimitiveType.TriangleList,
            RasterizerState = RasterizerState.CCW_CullNone,
            VertexShader = instancedPSC,
            FragmentShader = fragmentPSC,
            VertexInputState = new VertexInputState([
                vertexBufferDescription,
                instancedBufferDescription
            ])
        };

        InstancedPipeline = new GraphicsPipeline(device, instancedPipelineCreateInfo);
    }
}
