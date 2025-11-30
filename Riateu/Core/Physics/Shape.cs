using System.Numerics;
using System.Runtime.CompilerServices;
using Riateu.Graphics;

namespace Riateu.Physics;

/// <summary>
/// A base class for collision shapes.
/// </summary>
public abstract class Shape 
{
    public abstract Vector2 AbsoluteMin { get; }
    public abstract Vector2 AbsoluteMax { get; }

    /// <summary>
    /// A bounding hitbox of this shape.
    /// </summary>
    public RectangleF BoundingBox;
    /// <summary>
    /// An absolute bounding hitbox of this shape.
    /// </summary>
    public RectangleF AbsoluteBoundingBox 
    {
        get => new RectangleF(Entity.PosX + BoundingBox.X, Entity.PosY + BoundingBox.Y, BoundingBox.Width, BoundingBox.Height);
    }

    /// <summary>
    /// Base initialization for the collision shapes.
    /// </summary>
    /// <param name="entity">An <see cref="Riateu.Entity"/>. to hold</param>
    public Shape(Entity entity, RectangleF bound)  
    {
        Entity = entity;
        BoundingBox = bound;
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
    public abstract bool Collide(Vector2 position, RectangleF rect);
    
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
    /// Collide with any derived <see cref="Riateu.Physics.Shape"/>.
    /// </summary>
    /// <param name="position">An offset to adjust the collision</param>
    /// <param name="shape">A <see cref="Riateu.Physics.Shape"/> to collide with</param>
    /// <returns>true if it collided, false if it not collided</returns>
    public abstract bool Collide(Vector2 position, Shape shape);

    /// <summary>
    /// Draw a debug lines to show the hidden lines from this shape.
    /// </summary>
    /// <param name="buffer">A command buffer</param>
    /// <param name="draw">An <see cref="Riateu.Graphics.Batch"/></param>
    public virtual void DebugDraw(CommandBuffer buffer, Batch draw) 
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool Unsupported(Shape shape) 
    {
        Logger.Error($"Unsupported shape: '{shape.GetType().Name}'");
        return false;
    }
}
