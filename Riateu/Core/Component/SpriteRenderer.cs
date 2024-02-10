using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu.Components;

/// <summary>
/// A component used to render a sprite from a texture and quad.
/// </summary>
public class SpriteRenderer : GraphicsComponent
{
    /// <summary>
    /// Flip a quad horizontally
    /// </summary>
    public bool FlipX
    {
        get => (flip & FlipMode.Horizontal) == FlipMode.Horizontal;
        set 
        {
            flip |= value ? FlipMode.Horizontal : ~FlipMode.Horizontal;
            SpriteTexture.FlipUV(flip);
        }
    }

    /// <summary>
    /// Flip a quad vertically.
    /// </summary>
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

    /// <summary>
    /// An initilization for this component.
    /// </summary>
    /// <param name="baseTexture">A texture for the sprite</param>
    /// <param name="texture">A quad for the sprite</param>
    /// <returns></returns>
    public SpriteRenderer(Texture baseTexture, SpriteTexture texture) : base(texture, baseTexture)
    {
    }

    /// <inheritdoc/>
    public override void Draw(CommandBuffer buffer, IBatch batch) 
    {
        batch.Add(
            SpriteTexture, BaseTexture, GameContext.GlobalSampler, Vector2.Zero, 
            Color.White, Entity.Transform.WorldMatrix);
    }

    /// <inheritdoc/>
    public override void Update(double delta)
    {
    }
}