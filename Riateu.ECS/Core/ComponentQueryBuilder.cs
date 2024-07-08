using System.Collections.Generic;

namespace Riateu.ECS;

public struct ComponentQueryBuilder 
{
    private HashSet<TypeID> Includes;
    private HashSet<TypeID> Excludes;

    private World world;

    public ComponentQueryBuilder(World world) 
    {
        Includes = new HashSet<TypeID>();
        Excludes = new HashSet<TypeID>();
        this.world = world;
    }

    internal ComponentQueryBuilder(World world, HashSet<TypeID> includes, HashSet<TypeID> excludes) 
    {
        Includes = includes;
        Excludes = excludes;
        this.world = world;
    }

    public ComponentQueryBuilder Include<T>() 
    where T : unmanaged
    {
        TypeID id = world.GetComponentID<T>();
        Includes.Add(id);
        return new ComponentQueryBuilder(world, Includes, Excludes);
    }

    public ComponentQueryBuilder Exclude<T>() 
    where T : unmanaged
    {
        TypeID id = world.GetComponentID<T>();
        Includes.Add(id);
        return new ComponentQueryBuilder(world, Includes, Excludes);
    }

    public ComponentQuery Build() 
    {
        return world.BuildQuery(new ComponentQueryIndex(Includes, Excludes));
    }
}
