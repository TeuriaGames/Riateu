using System.Collections.Generic;

namespace Riateu.ECS;

public class ComponentQuery 
{
    private List<EntityID> entities = new List<EntityID>();
    private World world;
    private ComponentQueryIndex index;

    public List<EntityID> Entities => entities;
    
    public ComponentQuery(World world, ComponentQueryIndex index) 
    {
        this.world = world;
        this.index = index;
    }

    public void Update(EntityID entity) 
    {
        foreach (var type in index.Includes) 
        {
            if (!world.HasComponent(entity, type)) 
            {
                entities.Remove(entity);
                return;
            }
        }

        foreach (var type in index.Excludes) 
        {
            if (world.HasComponent(entity, type)) 
            {
                entities.Remove(entity);
                return;
            }
        }

        entities.Add(entity);
    }

    public void Remove(EntityID entity) 
    {
        entities.Remove(entity);
    }
}
