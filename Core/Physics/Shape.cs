using System;
using MoonWorks.Math.Float;

namespace Riateu.Physics;

public abstract class Shape 
{
    public abstract bool Collide(Vector2 position, Rectangle rect);
    public abstract bool Collide(Vector2 position, Vector2 point);
    public abstract bool Collide(Vector2 position, Point point);
    public abstract bool Collide(Vector2 position, AABB aabb);

    public bool Collide(Vector2 position, Shape shape) 
    {
        return shape switch 
        {
            AABB aabb => Collide(position, aabb),
            _ => throw new NotImplementedException()
        };
    }
}

public class AABB : Shape
{
    public Rectangle BoundingBox;
    public Entity Entity;
    

    public AABB(Entity entity, Rectangle rectangle) 
    {
        BoundingBox = rectangle;
        Entity = entity;
    }

    public AABB(Entity entity, int x, int y, int width, int height) 
        : this(entity, new Rectangle(x, y, width, height)) {}

    public override bool Collide(Vector2 position, Rectangle rect) 
    {
        return GetAbsoluteBounds().Intersects(rect);
    }

    public override bool Collide(Vector2 position, Vector2 point) => 
        Collide(position, new Point((int)point.X, (int)point.Y));

    public override bool Collide(Vector2 position, Point point) {
        return GetAbsoluteBounds().Contains(point);
    } 

    public override bool Collide(Vector2 position, AABB aabb) {
        return GetAbsoluteBounds().Intersects(aabb.GetAbsoluteBounds());
    } 

    private Rectangle GetAbsoluteBounds() 
    {
        return new Rectangle(
            (int)Entity.Position.X + BoundingBox.X,
            (int)Entity.Position.Y + BoundingBox.Y,
            BoundingBox.Width,
            BoundingBox.Height
        );
    }
}