using System.IO;
using MoonWorks.Graphics;

namespace Riateu.Misc;

public static class Resources
{
    private static byte[] instancedShader;
    public static byte[] InstancedShader
    {
        get
        {
            if (instancedShader == null)
            {
                instancedShader = GetShaderByte("InstancedShader.vert");
            }
            return instancedShader;
        }
    }

    private static byte[] imGuiShader;
    public static byte[] ImGuiShader
    {
        get
        {
            if (imGuiShader == null)
            {
                imGuiShader = GetShaderByte("ImGuiShader.vert");
            }
            return imGuiShader;
        }
    }

    private static byte[] positionTextureColor;
    public static byte[] PositionTextureColor
    {
        get
        {
            if (positionTextureColor == null)
            {
                positionTextureColor = GetShaderByte("PositionTextureColor.vert");
            }
            return positionTextureColor;
        }
    }

    private static byte[] texture;
    public static byte[] Texture
    {
        get
        {
            if (texture == null)
            {
                texture = GetShaderByte("Texture.frag");
            }
            return texture;
        }
    }

    private static string GetBackendName()
    {
#if D3D11
        return "D3D11";
#elif Metal
        return "Metal";
#else
        return "Vulkan";
#endif
    }

    private static byte[] GetShaderByte(string name)
    {
        string backend = GetBackendName();
        Stream stream = typeof(Resources).Assembly.GetManifestResourceStream(
            $"Riateu.Misc.{backend}.{name}.spv"
        );
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public static Shader GetShader(GraphicsDevice device, byte[] shader, string entryPoint, ShaderCreateInfo info)
    {
        using var ms = new MemoryStream(shader);
        var shaderModule = new Shader(device, ms, entryPoint, info);
        return shaderModule;
    }
}
