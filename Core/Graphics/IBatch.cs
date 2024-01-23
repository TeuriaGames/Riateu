using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu;

public interface IBatch 
{
    void Add(
        Texture texture, Sampler sampler, Vector2 position, Matrix3x2 transform,
        FlipMode flipMode = FlipMode.None, float layerDepth = 1) 
    {
        Add(new SpriteTexture(texture), texture, sampler, position, transform, flipMode, layerDepth);
    }

    void Add(
        SpriteTexture sTexture, Texture texture, Sampler sampler, Vector2 position, Matrix3x2 transform,
        FlipMode flipMode = FlipMode.None, float layerDepth = 1);

    void Draw(CommandBuffer cmdBuf);
    void Draw(CommandBuffer cmdBuf, Matrix4x4 viewProjection);

    void FlushVertex(CommandBuffer cmdBuf);
}