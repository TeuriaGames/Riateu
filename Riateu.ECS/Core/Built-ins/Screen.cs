using MoonWorks.Graphics;

namespace Riateu.ECS;


public class Screen : GameLoop
{
    private World world;

    public World World => world;

    public Screen(GameApp game) : base(game) 
    {
        world = new World();
    }

    public override void Begin()
    {
    }

    public override void End()
    {
    }

    public override void Update(double delta)
    {
        World.Refresh();
    }

    public override void Render(CommandBuffer buffer, Texture backbuffer)
    {
    }
}
