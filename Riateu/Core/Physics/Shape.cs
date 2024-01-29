using System;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu.Physics;

public abstract class Shape 
{
    public Shape(Entity entity)  
    {
        Entity = entity;
    }
    public Entity Entity;
    public abstract Shape Clone();
    

    public abstract bool Collide(Vector2 position, Rectangle rect);
    public abstract bool Collide(Vector2 position, Vector2 point);
    public abstract bool Collide(Vector2 position, Point point);
    public abstract bool Collide(Vector2 position, AABB aabb);
    public abstract bool Collide(Vector2 position, CollisionGrid grid);

    public bool Collide(Vector2 position, Shape shape) 
    {
        return shape switch 
        {
            AABB aabb => Collide(position, aabb),
            CollisionGrid grid => Collide(position, grid),
            _ => throw new NotImplementedException()
        };
    }

    public virtual void DebugDraw(CommandBuffer buffer, InstanceBatch batch) 
    {
    }
}
