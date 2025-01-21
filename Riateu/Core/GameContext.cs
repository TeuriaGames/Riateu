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
    public static Material BatchMaterial;
    /// <summary>
    /// A rendering material that uses R8G8B8A8 format.
    /// </summary>
    public static Material DepthMaterial;

    public static Shader SpriteBatchShader;
    public static Shader MSDFShader;

    internal static void Init(GraphicsDevice device, Window mainWindow)
    {
        GraphicsDevice = device;
        using var msdf = new MemoryStream(Resources.MSDF);
        MSDFShader = new Shader(device, msdf, "main", new ShaderCreateInfo {
            ShaderStage = ShaderStage.Fragment,
            ShaderFormat = GraphicsDevice.BackendShaderFormat,
            UniformBufferCount = 1,
            SamplerCount = 1
        });

        var spriteBatchShader = Resources.SpriteBatchShader;
        using var ms1 = new MemoryStream(spriteBatchShader);
        SpriteBatchShader = new Shader(device, ms1, "main", new ShaderCreateInfo {
            ShaderStage = ShaderStage.Vertex,
            ShaderFormat = GraphicsDevice.BackendShaderFormat,
            UniformBufferCount = 1,
            StorageBufferCount = 1
        });

        var textureFragment = Resources.Texture;
        using var ms2 = new MemoryStream(textureFragment);
        Shader fragmentPSC = new Shader(device, ms2, "main", new ShaderCreateInfo {
            ShaderStage = ShaderStage.Fragment,
            ShaderFormat = GraphicsDevice.BackendShaderFormat,
            SamplerCount = 1
        });

        BatchMaterial = new Material(device, GraphicsPipeline.CreateBuilder(SpriteBatchShader, fragmentPSC)
            .SetAttachmentInfo(new GraphicsPipelineAttachmentInfo(
                new ColorAttachmentDescription(mainWindow.SwapchainFormat,
                ColorTargetBlendState.AlphaBlend)
            ))
            .SetDepthStenctilState(DepthStencilState.DepthReadWrite)
            .SetMultisampleState(MultisampleState.None)
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .SetRasterizerState(RasterizerState.CCW_CullNone)

            .Build(device)
        );

        DepthMaterial = new Material(device, GraphicsPipeline.CreateBuilder(SpriteBatchShader, fragmentPSC)
            .SetAttachmentInfo(new GraphicsPipelineAttachmentInfo(
                TextureFormat.D24_UNORM,
                new ColorAttachmentDescription(mainWindow.SwapchainFormat, ColorTargetBlendState.AlphaBlend)
            ))
            .SetDepthStenctilState(DepthStencilState.DepthReadWrite)
            .SetMultisampleState(MultisampleState.None)
            .SetPrimitiveType(PrimitiveType.TriangleList)
            .SetRasterizerState(RasterizerState.CCW_CullNone)

            .Build(device)
        );
    }
}
