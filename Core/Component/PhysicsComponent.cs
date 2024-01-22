using System;
using Riateu.Physics;

namespace Riateu.Components;

public class PhysicsComponent : Component 
{
    private Shape shape;
    private Action<Entity, PhysicsComponent> onCollided;
    public PhysicsComponent(Shape shape, Action<Entity, PhysicsComponent> onCollided = null) 
    {
        this.shape = shape;
        this.onCollided = onCollided;
    }

    public bool Check(PhysicsComponent other) 
    {
        if (onCollided == null)
            return false;
        if (other.shape.Collide(Entity.Position, shape)) 
        {
            onCollided(other.Entity, other);
            return true;
        }

        return false;
    }

    public override void Added(Entity entity)
    {
        base.Added(entity);
        if (entity.Scene != null) 
        {
            entity.Scene.AddPhysics(this);
        }
    }

    public override void EntityEntered(Scene scene)
    {
        base.EntityEntered(scene);
        scene.AddPhysics(this);
    }

    public override void EntityExited(Scene scene)
    {
        scene.RemovePhysics(this);
        base.EntityExited(scene);
    }
}