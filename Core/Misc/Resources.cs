using System.IO;

namespace Riateu.Misc;

public static class Resources 
{
    private static byte[] positionTextureColor;
    public static byte[] PositionTextureColor 
    {
        get 
        {
            if (positionTextureColor == null) 
            {
                positionTextureColor = GetShader("PositionTextureColor.wgsl");
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
                texture = GetShader("Texture.wgsl");
            }
            return texture;
        }
    }

    private static byte[] GetShader(string name) 
    {
        Stream stream = typeof(Resources).Assembly.GetManifestResourceStream(
            "Riateu.Misc.Refresh." + name + ".refresh"
        );
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}