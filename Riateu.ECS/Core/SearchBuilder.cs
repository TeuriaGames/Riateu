using System.Collections.Generic;

namespace Riateu.ECS;

public struct SearchBuilder 
{
    private HashSet<TypeID> Withs;
    private HashSet<TypeID> Withouts;

    private World world;

    public SearchBuilder(World world) 
    {
        Withs = new HashSet<TypeID>();
        Withouts = new HashSet<TypeID>();
        this.world = world;
    }

    internal SearchBuilder(World world, HashSet<TypeID> withs, HashSet<TypeID> withouts) 
    {
        Withs = withs;
        Withouts = withouts;
        this.world = world;
    }

    public SearchBuilder With<T>() 
    where T : unmanaged
    {
        TypeID id = world.GetComponentID<T>();
        Withs.Add(id);
        return new SearchBuilder(world, Withs, Withouts);
    }

    public SearchBuilder Without<T>() 
    where T : unmanaged
    {
        TypeID id = world.GetComponentID<T>();
        Withs.Add(id);
        return new SearchBuilder(world, Withs, Withouts);
    }

    public SearchResult Confirm() 
    {
        return world.ConfirmResult(new SearchResultIndex(Withs, Withouts));
    }
}
