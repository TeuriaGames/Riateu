using System.Collections.Generic;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.ECS.Components;
using Riateu.Graphics;

namespace Riateu.ECS;

public class CollisionSystem : UpdateSystem
{
    private World world;
    public ComponentQuery CollisionUpdate;
    public ComponentQuery AddCollision;
    public Dictionary<EntityID, List<EntityID>> HitList = new Dictionary<EntityID, List<EntityID>>();

    public CollisionSystem(World world) : base(world)
    {
        this.world = world;

        CollisionUpdate = world.QueryBuilder
            .Include<Vector2>()
            .Include<Hitbox>()
            .Build();
        
        AddCollision = world.QueryBuilder
            .Include<Hitbox>()
            .Build();
    }

    public void OnEntitiesAdded() 
    {
        foreach (var entity in AddCollision.Entities) 
        {
            HitList.Add(entity, new List<EntityID>());
        }
    }

    public override void Update(double delta) 
    {
        foreach (var entity in CollisionUpdate.Entities) 
        {
            HitList[entity].Clear();
            ref Vector2 position = ref world.GetComponent<Vector2>(entity);
            ref Hitbox hitbox = ref world.GetComponent<Hitbox>(entity);

            Rectangle rect = new Rectangle((int)position.X, (int)position.Y, hitbox.shape.Width, hitbox.shape.Height);
            foreach (var another in CollisionUpdate.Entities) 
            {
                if (entity.id == another.id) 
                {
                    continue;
                }
                ref Vector2 anotherPosition = ref world.GetComponent<Vector2>(another);
                ref Hitbox anotherHitbox = ref world.GetComponent<Hitbox>(another);

                if (rect.Intersects(new Rectangle((int)anotherPosition.X, (int)anotherPosition.Y, anotherHitbox.shape.Width, anotherHitbox.shape.Height))) 
                {
                    HitList[entity].Add(another);
                }
            }
        }
    }
}

public class RendererSystem : DrawSystem
{
    private World world;
    private ComponentQuery DisplaySprite;

    public RendererSystem(World world) : base(world)
    {
        this.world = world;

        DisplaySprite = world.QueryBuilder
            .Include<Vector2>()
            .Include<SpriteRenderer>()
            .Build();
    }

    public override void Draw(CommandBuffer buffer, Batch batch) 
    {
        foreach (var entity in DisplaySprite.Entities) 
        {
            ref Vector2 position = ref world.GetComponent<Vector2>(entity);
            ref SpriteRenderer sprite = ref world.GetComponent<SpriteRenderer>(entity);
            batch.Draw(sprite.Texture, position, Color.White);
        }
    }
}