using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu.Components;

public class SpriteRendererComponent : Component
{
    private SpriteTexture texture;
    private Texture baseTexture;

    public SpriteRendererComponent(Texture baseTexture, SpriteTexture texture) 
    {
        this.texture = texture;
        this.baseTexture = baseTexture;
    }

    public override void Draw(Batch batch) 
    {
        batch.Add(texture, baseTexture, GameContext.GlobalSampler, Vector2.Zero, Entity.Transform.WorldMatrix);
    }

    public override void Update(double delta)
    {
    }
}