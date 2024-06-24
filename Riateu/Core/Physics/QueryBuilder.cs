using System;
using System.Collections.Generic;

namespace Riateu;

public struct QueryBuilder
{
    private Scene scene;
    public HashSet<uint> Includes;

    internal QueryBuilder(Scene scene) 
    {
        this.scene = scene;
        this.Includes = new();
    }

    private QueryBuilder(Scene scene, HashSet<uint> includes) 
    {
        this.scene = scene;
        this.Includes = includes;
    }

    public QueryBuilder Include<T>() 
    {
        Type type = typeof(T);
        if (scene.TypeIndexes.TryGetValue(type, out uint val)) 
        {
            Includes.Add(val);
        }
        return new QueryBuilder(scene, Includes);
    }

    public QueryResult Build() 
    {
        return scene.GetQuery(new Query(Includes));
    }
}
