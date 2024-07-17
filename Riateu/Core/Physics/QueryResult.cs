using System.Collections.Generic;
using Riateu.Components;
using Riateu.Physics;

namespace Riateu;

public class QueryResult
{
    private QueryBasePhysics queryBase;
    private Query query;

    public List<PhysicsComponent> Components = new();

    public QueryResult(QueryBasePhysics queryBase, Query query) 
    {
        this.queryBase = queryBase;
        this.query = query;
    }

    public void Add(List<PhysicsComponent> component) 
    {
        Components.AddRange(component);
    }

    public void Add(PhysicsComponent component) 
    {
        Components.Add(component);
    }

    public void Remove(PhysicsComponent component) 
    {
        Components.Remove(component);
    }
}
