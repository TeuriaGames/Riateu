using System.Collections.Generic;
using Riateu.Components;

namespace Riateu;

public class PhysicsStorage 
{
    public Dictionary<uint, List<PhysicsComponent>> PhysicsQuery = new();
    public Dictionary<uint, QueryResult> QueryResults = new();

    public void Add(uint indexCount, PhysicsComponent component) 
    {
        if (PhysicsQuery.TryGetValue(indexCount, out var list)) 
        {
            list.Add(component);
            if (QueryResults.TryGetValue(indexCount, out var res)) 
            {
                res.Add(component);
            }
            return;
        }

        PhysicsQuery.Add(indexCount, [component]);
    }

    public void Remove(uint indexCount, PhysicsComponent component) 
    {
        PhysicsQuery[indexCount].Remove(component);
        if (QueryResults.TryGetValue(indexCount, out var res)) 
        {
            res.Remove(component);
        }
    }
}
