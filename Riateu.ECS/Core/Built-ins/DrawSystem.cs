using MoonWorks.Graphics;
using Riateu.Graphics;

namespace Riateu.ECS;

public abstract class DrawSystem : System
{
    public DrawSystem(World world) : base(world)
    {
    }

    public abstract void Draw(CommandBuffer buffer, Batch batch);
}