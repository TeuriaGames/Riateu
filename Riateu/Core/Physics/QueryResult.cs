using System.Collections.Generic;
using Riateu.Components;

namespace Riateu;

public class QueryResult
{
    private Scene scene;
    private Query query;

    public List<PhysicsComponent> Components = new();

    public QueryResult(Scene scene, Query query) 
    {
        this.scene = scene;
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
