using System;
using System.Collections.Generic;

namespace Riateu.ECS;

public struct ComponentQueryIndex 
{
    public HashSet<TypeID> Includes;
    public HashSet<TypeID> Excludes;

    public ComponentQueryIndex(HashSet<TypeID> includes, HashSet<TypeID> excludes) 
    {
        this.Includes = includes;
        this.Excludes = excludes;
    }

    public override int GetHashCode()
    {
        int hashCode = 1;
        foreach (TypeID include in Includes) 
        {
            hashCode = HashCode.Combine(hashCode, include.id);
        }

        foreach (TypeID exclude in Excludes) 
        {
            hashCode = HashCode.Combine(hashCode, exclude.id);
        }
        return hashCode;
    }
}
