using System.IO;

namespace Riateu.Misc;

internal static class Resources 
{
    private static byte[] positionColorTexture;
    public static byte[] PositionColorTexture 
    {
        get 
        {
            if (positionColorTexture == null) 
            {
                positionColorTexture = GetShader("PositionColorTexture.wgsl");
            }
            return positionColorTexture;
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