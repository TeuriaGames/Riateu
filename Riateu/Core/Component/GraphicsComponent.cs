using Riateu.Graphics;

namespace Riateu.Components;

/// <summary>
/// A base class for graphics related component.
/// </summary>
public abstract class GraphicsComponent : Component
{
    /// <summary>
    /// A quad for this component.
    /// </summary>
    public TextureQuad SpriteTexture;

    /// <summary>
    /// An initilization for this component.
    /// </summary>
    /// <param name="texture">A quad for the component</param>
    /// <param name="baseTexture">A texture for the component</param>
    public GraphicsComponent(TextureQuad texture) 
    {
        SpriteTexture = texture;
    }

    public GraphicsComponent() 
    {
    }
}