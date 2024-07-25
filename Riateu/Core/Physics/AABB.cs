using System.Numerics;
using Riateu.Graphics;

namespace Riateu.Physics;

/// <summary>
/// A shape that uses for collision detection in a box shaped.
/// </summary>
public class AABB : Shape
{
    public override Vector2 Min => new Vector2(Entity.PosX + BoundingBox.X, Entity.PosY + BoundingBox.Y);
    public override Vector2 Max => new Vector2(Entity.PosX + BoundingBox.X + BoundingBox.Width, Entity.PosY + BoundingBox.Y + BoundingBox.Height);
    /// <summary>
    /// A bounding hitbox of this shape.
    /// </summary>
    public Rectangle BoundingBox;
    

    /// <summary>
    /// Initialization of this shape.
    /// </summary>
    /// <param name="entity">An <see cref="Riateu.Entity"/> to reference with</param>
    /// <param name="rectangle">A hitbox of this shape</param>
    public AABB(Entity entity, Rectangle rectangle) : base(entity)
    {
        BoundingBox = rectangle;
    }

    /// <summary>
    /// Initialization of this shape.
    /// </summary>
    /// <param name="entity">An <see cref="Riateu.Entity"/> to reference with</param>
    /// <param name="x">An x-axis of the hitbox</param>
    /// <param name="y">A y-axis of the hitbox</param>
    /// <param name="width">A width of the hitbox</param>
    /// <param name="height">A height of the hitbox</param>
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