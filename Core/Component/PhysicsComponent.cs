using System;
using System.Collections.Generic;
using MoonWorks.Math.Float;
using Riateu.Physics;

namespace Riateu.Components;

public class PhysicsComponent : Component 
{
    private Shape shape;
    private Action<Entity, PhysicsComponent> onCollided;

    public int Tags 
    {
        get => tags;
        set 
        {
            tags = value;
        }
    }

    private int tags = -1;
    private bool hasSceneTags;


    public PhysicsComponent(Shape shape, Action<Entity, PhysicsComponent> onCollided = null) 
    {
        this.shape = shape;
        this.onCollided = onCollided;
    }

    public bool Check(PhysicsComponent other, Vector2 offset) 
    {
        if (other == this)
            return false;
        if (shape.Collide(offset, other.shape)) 
        {
            return true;
        }

        return false;
    }

    public bool Check(PhysicsComponent other, Vector2 offset, Action<Entity, PhysicsComponent> onCollided) 
    {
        if (onCollided == null || other == this)
            return false;
        if (shape.Collide(offset, other.shape)) 
        {
            onCollided(other.Entity, other);
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

    private bool physicsAdded;

    public override void Added(Entity entity)
    {
        base.Added(entity);
        if (entity.Scene != null) 
        {
            entity.Scene.AddBit(this);
            entity.Scene.AddPhysics(this);
        }
    }

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

    public override void EntityExited(Scene scene)
    {
        scene.RemoveBit(this);
        scene.RemovePhysics(this);
        base.EntityExited(scene);
    }
}