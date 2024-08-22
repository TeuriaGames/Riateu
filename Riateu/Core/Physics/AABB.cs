using System.Numerics;
using Riateu.Graphics;

namespace Riateu.Physics;

/// <summary>
/// A shape that uses for collision detection in a box shaped.
/// </summary>
public class AABB : Shape
{
    public override Vector2 AbsoluteMin => new Vector2(Entity.PosX + BoundingBox.X, Entity.PosY + BoundingBox.Y);
    public override Vector2 AbsoluteMax => new Vector2(Entity.PosX + BoundingBox.X + BoundingBox.Width, Entity.PosY + BoundingBox.Y + BoundingBox.Height);


    /// <summary>
    /// Initialization of this shape.
    /// </summary>
    /// <param name="entity">An <see cref="Riateu.Entity"/> to reference with</param>
    /// <param name="rectangle">A hitbox of this shape</param>
    public AABB(Entity entity, RectangleF rectangle) : base(entity, rectangle) {}

    /// <summary>
    /// Initialization of this shape.
    /// </summary>
    /// <param name="entity">An <see cref="Riateu.Entity"/> to reference with</param>
    /// <param name="rectangle">A hitbox of this shape</param>
    public AABB(Entity entity, Rectangle rectangle) : base(entity, rectangle.ToFloat()) {}
    

    /// <summary>
    /// Initialization of this shape.
    /// </summary>
    /// <param name="entity">An <see cref="Riateu.Entity"/> to reference with</param>
    /// <param name="x">An x-axis of the hitbox</param>
    /// <param name="y">A y-axis of the hitbox</param>
    /// <param name="width">A width of the hitbox</param>
    /// <param name="height">A height of the hitbox</param>
    public AABB(Entity entity, int x, int y, int width, int height) 
        : this(entity, new RectangleF(x, y, width, height)) {}


    /// <inheritdoc/>
    public override Shape Clone()
    {
        return new AABB(Entity, BoundingBox);
    }

    /// <inheritdoc/>
    public override bool Collide(Vector2 position, Rectangle rect) 
    {
        var offsetRect = new RectangleF(rect.X + position.X, rect.Y + position.Y, rect.Width, rect.Height);
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
    public override bool Collide(Vector2 position, Shape shape) {
        switch (shape) 
        {
        case AABB:
            var absoluteBounds = shape.AbsoluteBoundingBox;
            return absoluteBounds.Intersects(GetAbsoluteBounds(position));
        case CollisionGrid grid:
            return grid.Collide(position, AbsoluteBoundingBox);
        case Circle circle:
            return circle.Collide(position, AbsoluteBoundingBox);
        default:
            return Unsupported(shape);
        }
    }

    /// <summary>
    /// Get an absolute bounds from an entity.
    /// </summary>
    /// <param name="offset">An offset to adjust the collision</param>
    /// <returns>
    /// A Rectangle based on both <see cref="Riateu.Entity"/>'s position 
    /// and this shape's position
    /// </returns>
    public RectangleF GetAbsoluteBounds(Vector2 offset = default) 
    {
        return new RectangleF(
            (int)Entity.Position.X + BoundingBox.X + (int)offset.X,
            (int)Entity.Position.Y + BoundingBox.Y + (int)offset.Y,
            BoundingBox.Width,
            BoundingBox.Height
        );
    }

    public override bool Collide(Vector2 position, RectangleF rect)
    {
        var offsetRect = new RectangleF(rect.X + position.X, rect.Y + position.Y, rect.Width, rect.Height);
        return GetAbsoluteBounds().Intersects(offsetRect);
    }
}