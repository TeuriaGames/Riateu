using System;

namespace Riateu.ECS;

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
