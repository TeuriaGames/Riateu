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
    /// <summary>
    /// A compute pipeline used for <see cref="Riateu.Graphics.Batch"/> to work.
    /// </summary>
    public static ComputePipeline SpriteBatchPipeline;

    internal static void Init(GraphicsDevice device, Window mainWindow)
    {
        GraphicsDevice = device;
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
            VertexInputState = new VertexInputState(
                VertexBinding.Create<PositionTextureColorVertex>(0),
                PositionTextureColorVertex.Attributes(0)
            )
        };

        DefaultMaterial = new Material(device, new GraphicsPipeline(device, pipelineCreateInfo));

        GraphicsPipelineCreateInfo depthCreateInfo = new GraphicsPipelineCreateInfo()
        {
            AttachmentInfo = new GraphicsPipelineAttachmentInfo(
                TextureFormat.D24_UNORM,
                new ColorAttachmentDescription(mainWindow.SwapchainFormat,
                ColorAttachmentBlendState.AlphaBlend)
            ),
            DepthStencilState = DepthStencilState.DepthReadWrite,
            MultisampleState = MultisampleState.None,
            PrimitiveType = PrimitiveType.TriangleList,
            RasterizerState = RasterizerState.CCW_CullNone,
            VertexShader = vertexPSC,
            FragmentShader = fragmentPSC,
            VertexInputState = new VertexInputState(
                VertexBinding.Create<PositionTextureColorVertex>(0),
                PositionTextureColorVertex.Attributes(0)
            )
        };

        DepthMaterial = new Material(device, new GraphicsPipeline(device, depthCreateInfo));

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
