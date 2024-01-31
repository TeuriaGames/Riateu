using System;
using MoonWorks.Math.Float;
using Riateu.Physics;

namespace Riateu.Components;

/// <summary>
/// A component that allows to have a collisions for all entities.
/// </summary>
public class PhysicsComponent : Component 
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
    public int Tags 
    {
        get => tags;
        set 
        {
            tags = value;
        }
    }

    private int tags = -1;

    /// <summary>
    /// Initialize the component with shapes.
    /// </summary>
    /// <param name="shape">A shape to use as a hitbox</param>

    public PhysicsComponent(Shape shape) 
    {
        this.shape = shape;
    }

    public bool Check(PhysicsComponent other, Vector2 offset) 
    {
        if (other == this || !other.Collidable)
            return false;
        if (shape.Collide(offset, other.shape)) 
        {
            return true;
        }

        return false;
    }

    public bool Check(PhysicsComponent other, Vector2 offset, Action<Entity, PhysicsComponent> onCollided) 
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

    public bool Check<T>(PhysicsComponent other, Vector2 offset, Action<T, PhysicsComponent> onCollided) 
    where T : Entity
    {
        if (onCollided == null || other == this || !other.Collidable)
            return false;
        if (shape.Collide(offset, other.shape)) 
        {
            onCollided(other.Entity as T, other);
            return true;
        }

        return false;
    }

    public bool CheckAll(Vector2 offset)
    {
        foreach (var other in Scene.PhysicsWorld.Components) 
        {
            if (Check(other, offset)) 
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckAll<T>(Vector2 offset, Tag tags, out T entity)
    where T : Entity
    {
        foreach (var other in Scene.GetPhysicsFromBit(tags)) 
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

    public bool CheckAll(Vector2 offset, out Entity entity, out PhysicsComponent component)
    {
        foreach (var other in Scene.PhysicsWorld.Components) 
        {
            if (Check(other, offset)) 
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

    public bool CheckAll(Vector2 offset, Tag tags)
    {
        foreach (var other in Scene.GetPhysicsFromBit(tags)) 
        {
            if (Check(other, offset)) 
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckAll(Vector2 offset, Tag tags, out Entity entity, out PhysicsComponent component)
    {
        foreach (var other in Scene.GetPhysicsFromBit(tags)) 
        {
            if (Check(other, offset)) 
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


    public bool OutsideCheckAll(Vector2 at)
    {
        foreach (var other in Scene.PhysicsWorld.Components) 
        {
            if (!Check(other, Vector2.Zero) && Check(other, at)) 
            {
                return true;
            }
        }
        return false;
    }

    public bool OutsideCheckAll<T>(Vector2 at, Tag tags, out T entity)
    where T : Entity
    {
        foreach (var other in Scene.GetPhysicsFromBit(tags)) 
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

    public bool OutsideCheckAll(Vector2 at, out Entity entity, out PhysicsComponent component)
    {
        foreach (var other in Scene.PhysicsWorld.Components) 
        {
            if (!Check(other, Vector2.Zero) && Check(other, at)) 
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

    public bool OutsideCheckAll(Vector2 at, Tag tags)
    {
        foreach (var other in Scene.GetPhysicsFromBit(tags)) 
        {
            if (!Check(other, Vector2.Zero) && Check(other, at)) 
            {
                return true;
            }
        }
        return false;
    }

    public bool OutsideCheckAll(Vector2 at, Tag tags, out Entity entity, out PhysicsComponent component)
    {
        foreach (var other in Scene.GetPhysicsFromBit(tags)) 
        {
            if (!Check(other, Vector2.Zero) && Check(other, at)) 
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
            entity.Scene.AddBit(this);
            entity.Scene.AddPhysics(this);
        }
    }

    /// <inheritdoc/>
    public override void EntityEntered(Scene scene)
    {
        base.EntityEntered(scene);
        if (!physicsAdded) 
        {
            scene.AddBit(this);
            scene.AddPhysics(this);
            physicsAdded = true;
        }
    }

    /// <inheritdoc/>
    public override void EntityExited(Scene scene)
    {
        scene.RemoveBit(this);
        scene.RemovePhysics(this);
        base.EntityExited(scene);
    }
}