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
            if (value) 
            {
                flip |= FlipMode.Horizontal;
            }
            else 
            {
                flip &= ~FlipMode.Horizontal;
            }
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
            if (value) 
            {
                flip |= FlipMode.Vertical;
            }
            else 
            {
                flip &= ~FlipMode.Vertical;
            }
            SpriteTexture.FlipUV(flip);
        }
    }

    private FlipMode flip;

    /// <summary>
    /// An origin of the texture.
    /// </summary>
    public Vector2 Origin;

    /// <summary>
    /// An initilization for this component.
    /// </summary>
    /// <param name="baseTexture">A texture for the sprite</param>
    /// <param name="texture">A quad for the sprite</param>
    /// <returns></returns>
    public SpriteRenderer(Texture baseTexture, TextureQuad texture) : base(texture, baseTexture)
    {
    }

    /// <inheritdoc/>
    public override void Draw(Batch draw) 
    {
        draw.Draw(SpriteTexture, Entity.Transform.Position, Color.White, Vector2.One, Entity.Transform.PivotOffset + Origin);
    }
}