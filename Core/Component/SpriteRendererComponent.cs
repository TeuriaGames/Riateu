using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu.Components;

public class SpriteRenderer : GraphicsComponent
{
    public bool Flip;
    public SpriteRenderer(Texture baseTexture, SpriteTexture texture) : base(texture, baseTexture)
    {
    }

    public override void Draw(CommandBuffer buffer, Batch batch) 
    {
        batch.Add(
            SpriteTexture, BaseTexture, GameContext.GlobalSampler, Vector2.Zero, 
            Entity.Transform.WorldMatrix, Flip ? FlipMode.Horizontal : FlipMode.None);
    }

    public override void Update(double delta)
    {
    }
}