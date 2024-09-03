using System.IO;
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
    /// The default material for basic rendering.
    /// </summary>
    public static Material DefaultMaterial;
    /// <summary>
    /// A rendering material that uses R8G8B8A8 format.
    /// </summary>
    public static Material DepthMaterial;

    public static Shader MSDFShader;
    /// <summary>
    /// A compute pipeline used for <see cref="Riateu.Graphics.Batch"/> to work.
    /// </summary>
    public static ComputePipeline SpriteBatchPipeline;

    internal static void Init(GraphicsDevice device, Window mainWindow)
    {
        GraphicsDevice = device;
        using var msdf = new MemoryStream(Resources.MSDF);
        MSDFShader = new Shader(device, msdf, "main", new ShaderCreateInfo {
            ShaderStage = ShaderStage.Fragment,
            ShaderFormat = BackendShaderFormat,
            UniformBufferCount = 1,
            SamplerCount = 1
        });

        var positionTextureColor = Resources.PositionTextureColor;
        using var ms1 = new MemoryStream(positionTextureColor);
        Shader vertexPSC = new Shader(device, ms1, "main", new ShaderCreateInfo {
            ShaderStage = ShaderStage.Vertex,
            ShaderFormat = BackendShaderFormat,
            UniformBufferCount = 1
        });

        var textureFragment = Resources.Texture;
        using var ms2 = new MemoryStream(textureFragment);
        Shader fragmentPSC = new Shader(device, ms2, "main", new ShaderCreateInfo {
            ShaderStage = ShaderStage.Fragment,
            ShaderFormat = BackendShaderFormat,
            SamplerCount = 1
        });

        DefaultMaterial = Material.CreateBuilder(vertexPSC, fragmentPSC)
            .SetAttachmentInfo(new GraphicsPipelineAttachmentInfo(
                new ColorAttachmentDescription(mainWindow.SwapchainFormat,
                ColorAttachmentBlendState.AlphaBlend)
            ))
            .SetDepthStenctilState(DepthStencilState.DepthReadWrite)
            .SetMultisampleState(MultisampleState.None)
            .SetPrimitiveType(PrimitiveType.TriangleList)
            .SetRasterizerState(RasterizerState.CCW_CullNone)

            .AddVertexInputState<PositionTextureColorVertex>()
            .Build(device);

        DepthMaterial = Material.CreateBuilder(vertexPSC, fragmentPSC)
            .SetAttachmentInfo(new GraphicsPipelineAttachmentInfo(
                TextureFormat.D24_UNORM,
                new ColorAttachmentDescription(mainWindow.SwapchainFormat, ColorAttachmentBlendState.AlphaBlend)
            ))
            .SetDepthStenctilState(DepthStencilState.DepthReadWrite)
            .SetMultisampleState(MultisampleState.None)
            .SetPrimitiveType(PrimitiveType.TriangleList)
            .SetRasterizerState(RasterizerState.CCW_CullNone)

            .AddVertexInputState<PositionTextureColorVertex>()
            .Build(device);

        var spriteBatchShader = Resources.SpriteBatchShader;
        using var comp1 = new MemoryStream(spriteBatchShader);
        SpriteBatchPipeline = new ComputePipeline(device, comp1, "main", new ComputePipelineCreateInfo 
        {
            ShaderFormat = BackendShaderFormat,
            ReadWriteStorageBufferCount = 1,
            ReadOnlyStorageBufferCount = 1,
            ThreadCountX = 64,
            ThreadCountY = 1,
            ThreadCountZ = 1
        });
    }

    /// <summary>
    /// A globally set shader format. Can be changed if you had a different backend format.
    /// </summary>
    public static ShaderFormat BackendShaderFormat = ShaderFormat.SPIRV;
}
