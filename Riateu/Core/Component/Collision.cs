using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Riateu.Graphics;
using Riateu.Physics;

namespace Riateu.Components;

/// <summary>
/// A component that allows to have a collisions for all entities.
/// </summary>
public class Collision : Component 
{
    /// <summary>
    /// A state whether this component should collide or not.
    /// </summary>
    public bool Collidable = true;
    private Shape shape;

    /// <summary>
    /// The shape that the component used. Changing its value will also change the hitbox.
    /// </summary>
    public Shape Shape 
    {
        get => shape;
        set => shape = value;
    }

    /// <summary>
    /// A physics scene tags to filter all the collisions in the scene tree.
    /// </summary>
    public ulong Tags 
    {
        get => tags;
        set 
        {
            tags = value;
        }
    }

    private ulong tags = 0;

    /// <summary>
    /// Initialize the component with shapes.
    /// </summary>
    /// <param name="shape">A shape to use as a hitbox</param>

    public Collision(Shape shape) 
    {
        this.shape = shape;
    }

    /// <summary>
    /// Check if this <see cref="Riateu.Components.Collision"/> is colliding with this component.
    /// </summary>
    /// <param name="other">A <see cref="Riateu.Components.Collision"/> to check with</param>
    /// <param name="offset">A coordinate offset for collision</param>
    /// <returns>true if it collided, else false</returns>
    public bool Check(Collision other, Vector2 offset) 
    {
        if (other == this || !other.Collidable)
            return false;
        if (shape.Collide(offset, other.shape)) 
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if this <see cref="Riateu.Components.Collision"/> is colliding with this component.
    /// </summary>
    /// <param name="other">A <see cref="Riateu.Components.Collision"/> to check with</param>
    /// <param name="offset">A coordinate offset for collision</param>
    /// <param name="onCollided">A callback that is called when any of the <see cref="Riateu.Components.Collision"/> collided</param>
    /// <returns>true if it collided, else false</returns>
    public bool Check(Collision other, Vector2 offset, Action<Entity, Collision> onCollided) 
    {
        if (onCollided == null || other == this || !other.Collidable)
            return false;
        if (shape.Collide(offset, other.shape)) 
        {
            onCollided(other.Entity, other);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if this <see cref="Riateu.Components.Collision"/> is colliding with this component.
    /// </summary>
    /// <param name="other">A <see cref="Riateu.Components.Collision"/> to check with</param>
    /// <param name="offset">A coordinate offset for collision</param>
    /// <param name="onCollided">A callback that is called when any of the <see cref="Riateu.Components.Collision"/> 
    /// that has <typeparamref name="T"/> <see cref="Riateu.Entity"/> collided</param>
    /// <typeparam name="T">An <see cref="Riateu.Entity"/> filter</typeparam>
    /// <returns>true if it collided, else false</returns>

    public bool Check<T>(Collision other, Vector2 offset, Action<T, Collision> onCollided) 
    where T : Entity
    {
        if (other.Entity is not T ent || onCollided == null || other == this || !other.Collidable)
            return false;
        if (shape.Collide(offset, other.shape)) 
        {
            onCollided(ent, other);
            return true;
        }

        return false;
    }

    public SpatialResult GetAllNearbyComponents(Vector2 offset) 
    {
        Rectangle rectangle = new Rectangle(
            (int)(Entity.PosX + offset.X),
            (int)(Entity.PosY + offset.Y),
            (int)(shape.BoundingBox.Width + (offset.X * 2)),
            (int)(shape.BoundingBox.Width + (offset.Y * 2))
        );
        return Scene.SpatialHash.Retrieve(rectangle, this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CompareTag(ulong otherTag, ulong tag) 
    {
        return (otherTag & tag) != 0;
    }

    public SpatialResult GetAllNearbyComponents(Vector2 offset, ulong tags) 
    {
        Rectangle rectangle = new Rectangle(
            (int)(Entity.PosX + offset.X),
            (int)(Entity.PosY + offset.Y),
            (int)(shape.BoundingBox.Width + (offset.X * 2)),
            (int)(shape.BoundingBox.Width + (offset.Y * 2))
        );
        return Scene.SpatialHash.Retrieve(rectangle, this, tags);
    }

    /// <summary>
    /// Check for all <see cref="Riateu.Components.Collision"/> if it's colliding with this component.
    /// </summary>
    /// <param name="offset">A coordinate offset for collision</param>
    /// <param name="tags">A <see cref="Riateu.Tag"/> filter to only check with these tags</param>
    /// <returns>true if it collided, else false</returns>
    public bool CheckAll(Vector2 offset, ulong tags)
    {
        using var components = GetAllNearbyComponents(offset);
        foreach (var other in components) 
        {
            if (CompareTag(other.tags, tags) && Check(other, offset)) 
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Check for all <see cref="Riateu.Components.Collision"/> if it's colliding with this component.
    /// </summary>
    /// <param name="offset">A coordinate offset for collision</param>
    /// <param name="entity">An output reference to an <see cref="Riateu.Entity"/> that has been collided with</param>
    /// <typeparam name="T">An entity filter</typeparam>
    /// <returns>true if it collided, else false</returns>
    public bool CheckAll<T>(Vector2 offset, out T entity)
    where T : Entity
    {
        using var components = GetAllNearbyComponents(offset);
        foreach (var other in components) 
        {
            if (other.Entity is T && Check(other, offset)) 
            {
                entity = (T)other.Entity;
                return true;
            }
        }
        entity = null;
        return false;
    }

    /// <summary>
    /// Check for all <see cref="Riateu.Components.Collision"/> if it's colliding with this component.
    /// </summary>
    /// <param name="offset">A coordinate offset for collision</param>
    /// <returns>true if it collided, else false</returns>
    public bool CheckAll<T>(Vector2 offset)
    {
        using var components = GetAllNearbyComponents(offset);
        foreach (var other in components) 
        {
            if (other.Entity is T && Check(other, offset)) 
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Check for all <see cref="Riateu.Components.Collision"/> if it's colliding with this component.
    /// </summary>
    /// <param name="offset">A coordinate offset for collision</param>
    /// <param name="tags">A <see cref="Riateu.Tag"/> filter to only check with these tags</param>
    /// <param name="entity">An output reference to an <see cref="Riateu.Entity"/> that has been collided with</param>
    /// <param name="component">An output reference to a <see cref="Riateu.Components.Collision"/> that has been collided with</param>
    /// <returns>true if it collided, else false</returns>
    public bool CheckAll(Vector2 offset, ulong tags, out Entity entity, out Collision component)
    {
        using var components = GetAllNearbyComponents(offset);
        foreach (var other in components) 
        {
            if (CompareTag(other.tags, tags) && Check(other, offset)) 
            {
                component = other;
                entity = other.Entity;
                return true;
            }
        }
        component = null;
        entity = null;
        return false;
    }

    /// <summary>
    /// Check for all <see cref="Riateu.Components.Collision"/> if it's colliding with this component.
    /// </summary>
    /// <param name="offset">A coordinate offset for collision</param>
    /// <param name="entity">An output reference to an <see cref="Riateu.Entity"/> that has been collided with</param>
    /// <param name="component">An output reference to a <see cref="Riateu.Components.Collision"/> that has been collided with</param>
    /// <returns>true if it collided, else false</returns>
    public bool CheckAll<T>(Vector2 offset, out Entity entity, out Collision component)
    {
        using var components = GetAllNearbyComponents(offset);
        foreach (var other in components) 
        {
            if (other.Entity is T && Check(other, offset)) 
            {
                component = other;
                entity = other.Entity;
                return true;
            }
        }
        component = null;
        entity = null;
        return false;
    }

    /// <summary>
    /// Check for all <see cref="Riateu.Components.Collision"/> if it's colliding with this component outside of its boundary.
    /// It works by checking its collision direction.
    /// </summary>
    /// <param name="at">A collision direction</param>
    /// <returns>true if it collided, else false</returns>
    public bool OutsideCheckAll<T>(Vector2 at)
    {
        using var components = GetAllNearbyComponents(at);
        foreach (var other in components) 
        {
            if (other.Entity is T && !Check(other, Vector2.Zero) && Check(other, at)) 
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Check for all <see cref="Riateu.Components.Collision"/> if it's colliding with this component outside of its boundary.
    /// It works by checking its collision direction.
    /// </summary>
    /// <param name="at">A collision direction</param>
    /// <param name="tags">A <see cref="Riateu.Tag"/> filter to only check with these tags</param>
    /// <returns>true if it collided, else false</returns>
    public bool OutsideCheckAll(Vector2 at, ulong tags)
    {
        using var components = GetAllNearbyComponents(at);
        foreach (var other in components) 
        {
            if (CompareTag(other.tags, tags) && !Check(other, Vector2.Zero) && Check(other, at)) 
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Check for all <see cref="Riateu.Components.Collision"/> if it's colliding with this component outside of its boundary.
    /// It works by checking its collision direction.
    /// </summary>
    /// <param name="at">A collision direction</param>
    /// <param name="entity">An output reference to an <see cref="Riateu.Entity"/> that has been collided with</param>
    /// <typeparam name="T">An entity filter</typeparam>
    /// <returns>true if it collided, else false</returns>
    public bool OutsideCheckAll<T>(Vector2 at, out T entity)
    where T : Entity
    {
        using var components = GetAllNearbyComponents(at);
        foreach (var other in components) 
        {
            if (other.Entity is T && (!Check(other, Vector2.Zero) && Check(other, at))) 
            {
                entity = (T)other.Entity;
                return true;
            }
        }
        entity = null;
        return false;
    }

    /// <summary>
    /// Check for all <see cref="Riateu.Components.Collision"/> if it's colliding with this component outside of its boundary.
    /// It works by checking its collision direction.
    /// </summary>
    /// <param name="at">A collision direction</param>
    /// <param name="entity">An output reference to an <see cref="Riateu.Entity"/> that has been collided with</param>
    /// <param name="component">An output reference to a <see cref="Riateu.Components.Collision"/> that has been collided with</param>
    /// <returns>true if it collided, else false</returns>
    public bool OutsideCheckAll<T>(Vector2 at, out Entity entity, out Collision component)
    {
        using var components = GetAllNearbyComponents(at);
        foreach (var other in components) 
        {
            if (other.Entity is T && !Check(other, Vector2.Zero) && Check(other, at)) 
            {
                component = other;
                entity = other.Entity;
                return true;
            }
        }
        component = null;
        entity = null;
        return false;
    }

    /// <summary>
    /// Check for all <see cref="Riateu.Components.Collision"/> if it's colliding with this component outside of its boundary.
    /// It works by checking its collision direction.
    /// </summary>
    /// <param name="at">A collision direction</param>
    /// <param name="tags">A <see cref="Riateu.Tag"/> filter to only check with these tags</param>
    /// <param name="entity">An output reference to an <see cref="Riateu.Entity"/> that has been collided with</param>
    /// <param name="component"></param>
    /// <returns>true if it collided, else false</returns>
    public bool OutsideCheckAll(Vector2 at, ulong tags, out Entity entity, out Collision component)
    {
        using var components = GetAllNearbyComponents(at);
        foreach (var other in components) 
        {
            if (CompareTag(other.tags, tags) && !Check(other, Vector2.Zero) && Check(other, at)) 
            {
                component = other;
                entity = other.Entity;
                return true;
            }
        }
        component = null;
        entity = null;
        return false;
    }

    private bool physicsAdded;

    /// <inheritdoc/>
    public override void Added(Entity entity)
    {
        base.Added(entity);
        if (entity.Scene != null) 
        {
            entity.Scene.AddPhysics(this);
        }
    }

    /// <inheritdoc/>
    public override void EntityEntered(Scene scene)
    {
        base.EntityEntered(scene);
        if (!physicsAdded) 
        {
            scene.AddPhysics(this);
            physicsAdded = true;
        }
    }

    /// <inheritdoc/>
    public override void EntityExited(Scene scene)
    {
        scene.RemovePhysics(this);
        base.EntityExited(scene);
    }

    /// <inheritdoc/>
    public override void Removed()
    {
        base.Removed();
        physicsAdded = false;
    }
}