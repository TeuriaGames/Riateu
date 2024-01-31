using MoonWorks.Math.Float;

namespace Riateu.Physics;

public class AABB : Shape
{
    public Rectangle BoundingBox;
    

    public AABB(Entity entity, Rectangle rectangle) : base(entity)
    {
        BoundingBox = rectangle;
    }

    public AABB(Entity entity, int x, int y, int width, int height) 
        : this(entity, new Rectangle(x, y, width, height)) {}

    /// <inheritdoc/>
    public override Shape Clone()
    {
        return new AABB(Entity, BoundingBox);
    }

    /// <inheritdoc/>
    public override bool Collide(Vector2 position, Rectangle rect) 
    {
        var offsetRect = new Rectangle(rect.X + (int)position.X, rect.Y + (int)position.Y, rect.Width, rect.Height);
        return GetAbsoluteBounds().Intersects(offsetRect);
    }

    /// <inheritdoc/>
    public override bool Collide(Vector2 position, Vector2 point) => 
        Collide(position, new Point((int)point.X, (int)point.Y));

    /// <inheritdoc/>
    public override bool Collide(Vector2 position, Point point) {
        var offsetPoint = new Point((int)(point.X + position.X), (int)(point.Y + position.Y));
        return GetAbsoluteBounds().Contains(offsetPoint);
    } 
    /// <inheritdoc/>
    public override bool Collide(Vector2 position, AABB aabb) {
        var absoluteBounds = aabb.GetAbsoluteBounds();
        return absoluteBounds.Intersects(GetAbsoluteBounds(position));
    }

    /// <inheritdoc/>
    public override bool Collide(Vector2 position, CollisionGrid grid)
    {
        return grid.Collide(position, this);
    }

    /// <summary>
    /// Get an absolute bounds from an entity.
    /// </summary>
    /// <param name="offset">An offset to adjust the collision</param>
    /// <returns>
    /// A Rectangle based on both <see cref="Riateu.Entity"/>'s position 
    /// and this shape's position
    /// </returns>
    public Rectangle GetAbsoluteBounds(Vector2 offset = default) 
    {
        return new Rectangle(
            (int)Entity.Position.X + BoundingBox.X + (int)offset.X,
            (int)Entity.Position.Y + BoundingBox.Y + (int)offset.Y,
            BoundingBox.Width,
            BoundingBox.Height
        );
    }
}