using System.Collections.Generic;

namespace Riateu;

public interface IPoolable<T> 
{
    void Created();
    void Destroy();
}

public static class Pool<T> 
where T : IPoolable<T>, new()
{
    private static Queue<T> pooled = new Queue<T>();

    public static void WarmUp(int count) 
    {
        count -= pooled.Count;
        if (count <= 0)
        {
            return;
        }

        for (int i = 0; i < count; i++) 
        {
            pooled.Enqueue(new T());
        }
    }

    public static T Create() 
    {
        if (pooled.Count > 0) 
        {
            T obj = pooled.Dequeue();
            obj.Created();
            return obj;
        }

        T newObj = new T();
        newObj.Created();
        return newObj;
    }

    public static void Destroy(T obj) 
    {
        pooled.Enqueue(obj);

        obj.Destroy();
    }

    public static void ClearCache() 
    {
        pooled.Clear();
    }
}