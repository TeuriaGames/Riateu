using System;
using System.Collections.Generic;

namespace Riateu.ECS;

public struct SearchResultIndex 
{
    public HashSet<TypeID> Withs;
    public HashSet<TypeID> Withouts;

    public SearchResultIndex(HashSet<TypeID> withs, HashSet<TypeID> withouts) 
    {
        this.Withs = withs;
        this.Withouts = withouts;
    }

    public override int GetHashCode()
    {
        int hashCode = 1;
        foreach (TypeID with in Withs) 
        {
            hashCode = HashCode.Combine(hashCode, with.id);
        }

        foreach (TypeID without in Withouts) 
        {
            hashCode = HashCode.Combine(hashCode, without.id);
        }
        return hashCode;
    }
}
