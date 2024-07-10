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
        World.SendMessage(message);
    }

    public T Receive<T>() 
    where T : unmanaged
    {
        return World.ReceiveMessage<T>();
    }

    public bool ReceiveSome<T>(out T message) 
    where T : unmanaged
    {
        if (!World.IsEmptyMessage<T>()) 
        {
            T inMessage = World.ReceiveMessage<T>();
            message = inMessage;
            return true;
        }

        message = default;
        return false;
    }

    public ReadOnlySpan<T> ReceiveAll<T>() 
    where T : unmanaged
    {
        return World.ReceiveAllMessage<T>();
    }

    public bool Some<T>() 
    where T : unmanaged
    {
        return !World.IsEmptyMessage<T>();
    }

    public bool None<T>() 
    where T : unmanaged
    {
        return World.IsEmptyMessage<T>();
    }
}
