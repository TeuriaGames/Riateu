namespace Riateu.ECS;

public abstract class System 
{
    private World world;
    public World World => world;
    public System(World world) 
    {
        this.world = world;
    }
}
