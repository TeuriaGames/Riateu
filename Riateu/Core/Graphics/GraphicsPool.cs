using System;
using System.Collections.Concurrent;

namespace Riateu.Graphics;

internal static class GraphicsPool<T>
where T : IGraphicsPool, new()
{
    private static ConcurrentQueue<T> pool = new ConcurrentQueue<T>();
    
    public static T Obtain(GraphicsDevice device) 
    {
        if (pool.TryDequeue(out T res)) 
        {
            res.Obtain(device);
            return res;
        }
        T t = new T();
        t.Obtain(device);
        return t;
    }

    public static void Release(T t) 
    {
        t.Reset();
        pool.Enqueue(t);
    }
}

internal static class PassPool<T>
where T : IPassPool, new()
{
    private static ConcurrentQueue<T> pool = new ConcurrentQueue<T>();
    
    public static T Obtain(IntPtr handle) 
    {
        if (pool.TryDequeue(out T res)) 
        {
            res.Obtain(handle);
            return res;
        }

        T pass = new T();
        pass.Obtain(handle);
        return pass;
    }

    public static void Release(T t) 
    {
        t.Reset();
        pool.Enqueue(t);
    }
}