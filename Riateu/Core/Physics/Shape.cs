using System;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu.Physics;

/// <summary>
/// A base class for collision shapes.
/// </summary>
public abstract class Shape 
{
    /// <summary>
    /// Base initialization for the collision shapes.
    /// </summary>
    /// <param name="entity">An <see cref="Riateu.Entity"/>. to hold</param>
    public Shape(Entity entity)  
    {
        Entity = entity;
    }

    /// <summary>
    /// A held <see cref="Riateu.Entity"/>.
    /// </summary>
    public Entity Entity;

    /// <summary>
    /// Clone a shape and create new reference.
    /// </summary>
    /// <returns>A exact same shape that is cloned</returns>
    public abstract Shape Clone();
    
    /// <summary>
    /// Collide with a Rectangle.
    /// </summary>
    /// <param name="position">An offset to adjust the collision</param>
    /// <param name="rect">A Rectangle to collide with</param>
    /// <returns>true if it collided, false if it not collided</returns>
    public abstract bool Collide(Vector2 position, Rectangle rect);
    /// <summary>
    /// Collide with a Vector2 point.
    /// </summary>
    /// <param name="position">An offset to adjust the collision</param>
    /// <param name="point">A Vector2 to collide with</param>
    /// <returns>true if it collided, false if it not collided</returns>
    public abstract bool Collide(Vector2 position, Vector2 point);
    /// <summary>
    /// Collide with a Point.
    /// </summary>
    /// <param name="position">An offset to adjust the collision</param>
    /// <param name="point">A Point to collide with</param>
    /// <returns>true if it collided, false if it not collided</returns>
    public abstract bool Collide(Vector2 position, Point point);
    /// <summary>
    /// Collide with an <see cref="Riateu.Physics.AABB"/>.
    /// </summary>
    /// <param name="position">An offset to adjust the collision</param>
    /// <param name="aabb">An <see cref="Riateu.Physics.AABB"/> to collide with</param>
    /// <returns>true if it collided, false if it not collided</returns>
    public abstract bool Collide(Vector2 position, AABB aabb);
    /// <summary>
    /// Collide with an <see cref="Riateu.Physics.CollisionGrid"/>.
    /// </summary>
    /// <param name="position">An offset to adjust the collision</param>
    /// <param name="grid">A <see cref="Riateu.Physics.CollisionGrid"/> to collide with</param>
    /// <returns>true if it collided, false if it not collided</returns>
    public abstract bool Collide(Vector2 position, CollisionGrid grid);

    /// <summary>
    /// Collide with any derived <see cref="Riateu.Physics.Shape"/>.
    /// </summary>
    /// <param name="position">An offset to adjust the collision</param>
    /// <param name="shape">A <see cref="Riateu.Physics.Shape"/> to collide with</param>
    /// <returns>true if it collided, false if it not collided</returns>
    public bool Collide(Vector2 position, Shape shape) 
    {
        return shape switch 
        {
            AABB aabb => Collide(position, aabb),
            CollisionGrid grid => Collide(position, grid),
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// Draw a debug lines to show the hidden lines from this shape.
    /// </summary>
    /// <param name="buffer">A command buffer</param>
    /// <param name="draw">An <see cref="Riateu.Graphics.Batch"/></param>
    public virtual void DebugDraw(CommandBuffer buffer, Batch draw) 
    {
    }
}
