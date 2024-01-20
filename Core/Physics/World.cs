using System.Collections.Generic;
using Riateu.Components;

namespace Riateu;

public class World 
{
    public HashSet<PhysicsComponent> Components = new();


    public void Insert(PhysicsComponent component) 
    {
        Components.Add(component);
    }

    public void Remove(PhysicsComponent component) 
    {
        Components.Remove(component);
    }

    public IEnumerable<PhysicsComponent> Retrieve(PhysicsComponent component) 
    {
        foreach (var comp in Components) 
        {
            if (comp == component)
                continue;
            yield return comp;
        }
    }
}