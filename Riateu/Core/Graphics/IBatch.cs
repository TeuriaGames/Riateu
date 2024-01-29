using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

public interface IBatch 
{
    void Start();
    void Add(
        Texture baseTexture, 
        Sampler sampler, 
        Vector2 position, 
        Matrix3x2 transform, 
        float layerDepth = 1);
    

    void Add(
        SpriteTexture sTexture, 
        Texture baseTexture, 
        Sampler sampler, 
        Vector2 position, 
        Matrix3x2 transform, 
        float layerDepth = 1); 

    void FlushVertex(CommandBuffer cmdBuf);
    void PushMatrix(in Matrix4x4 matrix);
    

    void PushMatrix(in Camera camera) 
    {
        PushMatrix(camera.Transform);
    }

    void PopMatrix();
    void Draw(CommandBuffer cmdBuf);
    void Draw(CommandBuffer cmdBuf, Matrix4x4 viewProjection);
    void End(CommandBuffer cmdBuf);
}
