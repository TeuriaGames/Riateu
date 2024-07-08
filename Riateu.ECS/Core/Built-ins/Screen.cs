using System;
using MoonWorks.Graphics;
using Riateu.Graphics;

namespace Riateu.ECS;

// TODO implement a GameLoop instead of a Scene
// TODO Remove Begin and End
public class Screen : Scene
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
        base.Update(delta);
        World.Refresh();
    }
}

public abstract class System 
{
    private World world;
    public World World => world;
    public System(World world) 
    {
        this.world = world;
    }
}

public abstract class UpdateSystem : System
{
    public UpdateSystem(World world) : base(world)
    {
    }

    public abstract void Update(double delta);

    public void Send<T>(T message) 
    where T : unmanaged
    {
        World.Send(message);
    }

    public T Receive<T>() 
    where T : unmanaged
    {
        return World.Receive<T>();
    }

    public ReadOnlySpan<T> ReceiveAll<T>() 
    where T : unmanaged
    {
        return World.ReceiveAll<T>();
    }
}

public abstract class DrawSystem : System
{
    public DrawSystem(World world) : base(world)
    {
    }

    public abstract void Draw(CommandBuffer buffer, Batch batch);
}