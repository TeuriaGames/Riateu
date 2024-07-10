namespace Riateu.ECS;

public abstract class System 
{
    private World world;
    public World World => world;
    public SearchBuilder Search => World.Search;

    public System(World world) 
    {
        this.world = world;
    }

    public void Add<T>(in EntityID entity, in T component) 
    where T : unmanaged
    {
        World.AddComponent(entity, component);
    }

    public ref T Get<T>(in EntityID entity) 
    where T : unmanaged
    {
        return ref World.GetComponent<T>(entity);
    }

    public bool Remove<T>(in EntityID entity) 
    where T : unmanaged
    {
        return World.RemoveComponent<T>(entity);
    }
}
