using System.IO;
using MoonWorks.Graphics;

namespace Riateu.Misc;

public static class Resources 
{
    private static byte[] tilemapShader;
    public static byte[] TilemapShader
    {
        get 
        {
            if (tilemapShader == null) 
            {
                tilemapShader = GetShaderByte("TilemapShader.wgsl");
            }
            return tilemapShader;
        }
    }

    private static byte[] imGuiShader;
    public static byte[] ImGuiShader 
    {
        get 
        {
            if (imGuiShader == null) 
            {
                imGuiShader = GetShaderByte("ImGuiShader.wgsl");
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
                positionTextureColor = GetShaderByte("PositionTextureColor.wgsl");
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
                texture = GetShaderByte("Texture.wgsl");
            }
            return texture;
        }
    }

    private static byte[] GetShaderByte(string name) 
    {
        Stream stream = typeof(Resources).Assembly.GetManifestResourceStream(
            "Riateu.Misc.Refresh." + name + ".refresh"
        );
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public static ShaderModule GetShader(GraphicsDevice device, byte[] shader) 
    {
        using var ms = new MemoryStream(shader);
        var shaderModule = new ShaderModule(device, ms);
        return shaderModule;
    }
}