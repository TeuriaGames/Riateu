using MoonWorks.Graphics;
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
    public Quad SpriteTexture;
    /// <summary>
    /// A texture for this component.
    /// </summary>
    public Texture BaseTexture;

    /// <summary>
    /// An initilization for this component.
    /// </summary>
    /// <param name="texture">A quad for the component</param>
    /// <param name="baseTexture">A texture for the component</param>
    public GraphicsComponent(Quad texture, Texture baseTexture) 
    {
        SpriteTexture = texture;
        BaseTexture = baseTexture;
    }

    /// <summary>
    /// An initilization for this component.
    /// </summary>
    /// <param name="baseTexture">A texture for the component</param>
    public GraphicsComponent(Texture baseTexture) 
    {
        BaseTexture = baseTexture;
    }
}