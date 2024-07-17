using System;
using System.Collections.Generic;

namespace Riateu;

public struct Query : IEquatable<Query>
{
    public readonly HashSet<uint> Includes;

    public Query(HashSet<uint> includes) 
    {
        Includes = includes;
    }

    public bool Equals(Query other)
    {
        foreach (var include in Includes) 
        {
            if (!other.Includes.Contains(include)) 
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        int hashcode = 1;

        foreach (var include in Includes) 
        {
            hashcode = HashCode.Combine(hashcode, include);
        }

        return hashcode;
    }
}
