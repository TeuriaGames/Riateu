using System.Collections.Generic;

namespace Riateu.ECS;

public class ComponentList
{
    public Dictionary<EntityID, int> Mappings = new Dictionary<EntityID, int>();
    public AnyOpaqueArray Components;
    public List<EntityID> Entities;

    public ComponentList(int elementSize) 
    {
        Components = new AnyOpaqueArray(elementSize);
        Entities = new List<EntityID>();
    }

    public bool Has<T>(in EntityID entity) 
    where T : unmanaged
    {
        return Mappings.ContainsKey(entity);
    }

    public bool Add<T>(in EntityID entity, in T component) 
    where T : unmanaged
    {
        if (Mappings.TryGetValue(entity, out int value)) 
        {
            Components.Set(value, component);
            return true;
        }
        Mappings[entity] = Components.Count;
        Components.Add(component);
        Entities.Add(entity);
        return false;
    }

    public ref T Get<T>(in EntityID entity) 
    where T : unmanaged
    {
        int index = Mappings[entity];
        return ref Components.Get<T>(index);
    }

    public bool Remove(EntityID entity) 
    {
        if (Mappings.TryGetValue(entity, out int value)) 
        {
            int lastElementIndex = Components.Count - 1;
            EntityID lastEntity = Entities[lastElementIndex];

            Components.Remove(value);
            Entities.RemoveAt(value);
            Mappings.Remove(entity);

            if (lastElementIndex != value) 
            {
                Mappings[lastEntity] = value;
            }
            return true;
        }
        return false;
    }
}
