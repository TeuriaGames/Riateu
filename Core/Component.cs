using System;
using MoonWorks.Graphics;
using Riateu.Graphics;

namespace Riateu;

public class Component
{
    internal static ulong id;
    public ulong ID { get; internal set; }
    public Scene Scene
    {
        get
        {
            if (this.Entity == null)
                throw new EntityDoesNotExistException();
            
            return this.Entity.Scene;
        }
    }
    public Entity Entity;
    public bool Active { get; set; }

    public Component() {}

    public virtual void Added(Entity entity) 
    {
        ID = id++;
        Entity = entity;
        Active = true;
    }

    public virtual void EntityEntered(Scene scene) 
    {
    }

    public virtual void EntityExited(Scene scene) 
    {
    }
    
    public virtual void Update(double delta) {}
    public virtual void Draw(CommandBuffer buffer, IBatch spriteBatch) {}
    public virtual void Removed() 
    {
        Entity = null;
    }

    public void DetachSelf() 
    {
        Entity?.RemoveComponent(this);
    }

    public override string ToString()
    {
        var typeName = GetType().Name;
        return $"[{typeName} {ID}]";
    }

    public void EnsureEntity<T>() 
    where T : Entity
    {
        if (Entity is not T) 
        {
            var type = typeof(T);
            var typeName = $"{type.Namespace}.{type.Name}";
            throw new Exception($"Wrong entity type for this component. Must be at type of '{typeName}'");
        }
    }
}

public class EntityDoesNotExistException : Exception 
{
    public EntityDoesNotExistException() {}
    public EntityDoesNotExistException(string message) : base(message) 
    {

    }
}