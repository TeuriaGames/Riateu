using System.Collections.Generic;
using Riateu.Graphics;

namespace Riateu.Content.App;
public class ImageCache(GraphicsDevice device)
{
    private Dictionary<string, Texture> pathToTexture = new Dictionary<string, Texture>();
    private GraphicsDevice device = device;


    public Texture LoadImage(string path) 
    {
        if (pathToTexture.TryGetValue(path, out Texture texture)) 
        {
            return texture;
        }
        Image image = new Image(path);
        using ResourceUploader uploader = new ResourceUploader(device);
        Texture tex = uploader.CreateTexture2D(image.Pixels, (uint)image.Width, (uint)image.Height);
        uploader.UploadAndWait();

        pathToTexture.Add(path, tex);
        return tex;
    }
}
