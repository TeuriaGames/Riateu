using System;

namespace Riateu.ECS;

public class MessageList 
{
    private AnyOpaqueArray messages;

    public MessageList(int elementSize) 
    {
        messages = new AnyOpaqueArray(elementSize);
    }

    public void Add<T>(T message) 
    where T : unmanaged
    {
        messages.Add(message);
    }

    public bool IsEmpty() 
    {
        return messages.Count == 0;
    }

    public ReadOnlySpan<T> ReadAll<T>() 
    where T : unmanaged
    {
        return messages.AsSpan<T>();
    }

    public ref T ReadFirst<T>() 
    where T : unmanaged
    {
        return ref messages.Get<T>(0);
    }

    public void Clear() 
    {
        messages.Clear();
    }
}