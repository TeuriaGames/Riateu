using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu.Components;

public class SpriteRenderer : GraphicsComponent
{

    public SpriteRenderer(Texture baseTexture, SpriteTexture texture) : base(texture, baseTexture)
    {
    }

    public override void Draw(Batch batch) 
    {
        batch.Add(SpriteTexture, BaseTexture, GameContext.GlobalSampler, Vector2.Zero, Entity.Transform.WorldMatrix);
    }

    public override void Update(double delta)
    {
    }
}