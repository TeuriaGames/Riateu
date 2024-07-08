using System;
using System.Collections.Generic;
using System.Diagnostics;
using Riateu.Components;

namespace Riateu.Physics;

public interface IPhysicsEngine 
{
    void AddPhysics(PhysicsComponent component);
    void RemovePhysics(PhysicsComponent component);

    List<PhysicsComponent> GetPhysicsComponents(PhysicsComponent component);
    List<PhysicsComponent> GetPhysicsComponents<T>(PhysicsComponent component);

    void Update();
    void Finish();
}

public class QueryBasePhysics : IPhysicsEngine
{
    internal static uint TypeIndexCount;
    public Dictionary<Type, uint> TypeIndexes = new();
    public PhysicsStorage PhysicsStorage = new();
    public Dictionary<Query, QueryResult> Queries = new();
    public QueryBuilder QueryBuilder => new QueryBuilder(this);


    public void AddPhysics(PhysicsComponent component)
    {
        Type type = component.Entity.GetType();
        uint indexCount;
        if (!TypeIndexes.TryGetValue(type, out indexCount)) 
        {
            indexCount = TypeIndexCount;
            TypeIndexes.Add(type, indexCount);
            TypeIndexCount++;
        }

        PhysicsStorage.Add(indexCount, component);
    }

    public void RemovePhysics(PhysicsComponent component)
    {
        Type type = component.Entity.GetType();

        uint indexCount = TypeIndexes[type];

        PhysicsStorage.Remove(indexCount, component);
    }

    public List<PhysicsComponent> GetPhysicsComponents(PhysicsComponent component)
    {
        throw new NotImplementedException();
    }

    public List<PhysicsComponent> GetPhysicsComponents<T>(PhysicsComponent component)
    {
        return QueryBuilder.Include<T>().Build().Components;
    }

    internal QueryResult GetQuery(Query query) 
    {
        QueryResult result;
        if (!Queries.TryGetValue(query, out result)) 
        {
            result = new QueryResult(this, query);

            foreach (var include in query.Includes) 
            {
                result.Add(PhysicsStorage.PhysicsQuery[include]);
                PhysicsStorage.QueryResults[include] = result;
            }
        }

        return result;
    }

    public void Update()
    {
    }

    public void Finish()
    {
    }
}

public class SpatialHashPhysics : IPhysicsEngine
{
    public List<PhysicsComponent> SpatialComponents = new List<PhysicsComponent>();
    public SpatialHash SpatialHash;

    public SpatialHashPhysics(int width, int height, int cellSize) 
    {
        SpatialHash = new SpatialHash(width, height, cellSize);
    }

    public void AddPhysics(PhysicsComponent component)
    {
        SpatialComponents.Add(component);
    }

    public void Finish()
    {
        SpatialHash.Clear();
    }

    public List<PhysicsComponent> GetPhysicsComponents(PhysicsComponent comp)
    {
        return SpatialHash.GetNearby(comp);
    }

    public List<PhysicsComponent> GetPhysicsComponents<T>(PhysicsComponent component)
    {
        return GetPhysicsComponents(component);
    }

    public void RemovePhysics(PhysicsComponent component)
    {
        SpatialComponents.Remove(component);
    }

    public void Update()
    {
        foreach (var physicsComp in SpatialComponents) 
        {
            SpatialHash.AddObject(physicsComp);
        }
    }
}