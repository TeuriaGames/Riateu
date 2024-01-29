using MoonWorks.Graphics;
using Riateu.Graphics;

namespace Riateu.Components;

public class GraphicsComponent : Component
{
    public SpriteTexture SpriteTexture;
    public Texture BaseTexture;

    public GraphicsComponent(SpriteTexture texture, Texture baseTexture) 
    {
        SpriteTexture = texture;
        BaseTexture = baseTexture;
    }

    public GraphicsComponent(Texture baseTexture) 
    {
        BaseTexture = baseTexture;
    }
}