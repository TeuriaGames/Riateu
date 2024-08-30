using System;
using System.Collections.Generic;

namespace Riateu;

public class EntityStorage
{
    public Dictionary<Type, WeakList<Entity>> Storages = new Dictionary<Type, WeakList<Entity>>();

    public void Add(Type type, Entity entity) 
    {
        if (Storages.TryGetValue(type, out var list)) 
        {
            list.Add(entity);
            return;
        }
        WeakList<Entity> entities = new WeakList<Entity>();
        entities.Add(entity);
        Storages.Add(type, entities);       
    }

    public void Add<T>(Entity entity) 
    where T : Component
    {
        Type type = typeof(T);
        Add(type, entity);
    }

    public void Remove(Type type, Entity entity) 
    {
        if (Storages.TryGetValue(type, out var list)) 
        {
            list.Remove(entity);
        }
    }

    public void Remove<T>(Entity entity) 
    where T : Component
    {
        Type type = typeof(T);
        Remove(type, entity);
    }

    public WeakEnumerator<Entity> GetAllEntitiesByComponents<T>() 
    where T : Component
    {
        Type type = typeof(T);
        if (Storages.TryGetValue(type, out var list)) 
        {
            return list.GetEnumerator();
        }
        return WeakEnumerator<Entity>.Empty;
    }
}