using System;
using System.Collections.Generic;
using Riateu.Physics;

namespace Riateu;

public struct QueryBuilder
{
    private QueryBasePhysics queryBase;
    public HashSet<uint> Includes;

    internal QueryBuilder(QueryBasePhysics queryBase) 
    {
        this.queryBase = queryBase;
        this.Includes = new();
    }

    private QueryBuilder(QueryBasePhysics queryBase, HashSet<uint> includes) 
    {
        this.queryBase = queryBase;
        this.Includes = includes;
    }

    public QueryBuilder Include<T>() 
    {
        Type type = typeof(T);
        if (queryBase.TypeIndexes.TryGetValue(type, out uint val)) 
        {
            Includes.Add(val);
        }
        return new QueryBuilder(queryBase, Includes);
    }

    public QueryResult Build() 
    {
        QueryResult queryResult = queryBase.GetQuery(new Query(Includes));
        return queryResult;
    }
}
