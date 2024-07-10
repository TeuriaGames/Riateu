namespace Riateu.ECS;

public class SearchResult 
{
    private WeakList<EntityID> entities = new WeakList<EntityID>();
    private World world;
    private SearchResultIndex index;

    public WeakEnumerator<EntityID> Entities => entities.GetEnumerator();
    
    public SearchResult(World world, SearchResultIndex index) 
    {
        this.world = world;
        this.index = index;
    }

    public void Update(EntityID entity) 
    {
        foreach (var type in index.Withs) 
        {
            if (!world.HasComponent(entity, type)) 
            {
                entities.Remove(entity);
                return;
            }
        }

        foreach (var type in index.Withouts) 
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
