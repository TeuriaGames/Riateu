using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu.Components;

public class SpriteRenderer : GraphicsComponent
{
    public bool FlipX
    {
        get => (flip & FlipMode.Horizontal) == FlipMode.Horizontal;
        set 
        {
            flip |= value ? FlipMode.Horizontal : ~FlipMode.Horizontal;
            SpriteTexture.FlipUV(flip);
        }
    }

    public bool FlipY
    {
        get => (flip & FlipMode.Vertical) == FlipMode.Vertical;
        set 
        {
            flip |= value ? FlipMode.Vertical : ~FlipMode.Vertical;
            SpriteTexture.FlipUV(flip);
        }
    }

    private FlipMode flip;

    public SpriteRenderer(Texture baseTexture, SpriteTexture texture) : base(texture, baseTexture)
    {
    }

    public override void Draw(CommandBuffer buffer, IBatch batch) 
    {
        batch.Add(
            SpriteTexture, BaseTexture, GameContext.GlobalSampler, Vector2.Zero, 
            Entity.Transform.WorldMatrix);
    }

    public override void Update(double delta)
    {
    }
}