using System;
using MoonWorks.Graphics;
using Riateu.Graphics;

namespace Riateu;

/// <summary>
/// A class that can define a behavior for an entity. This class should be added by an 
/// entity to function.
/// </summary>
public class Component
{
    internal static ulong id;
    /// <summary>
    /// An ID of a component.
    /// </summary>
    public ulong ID { get; internal set; }
    /// <summary>
    /// A current scene that the entity is alive on.
    /// </summary>
    public Scene Scene
    {
        get
        {
            if (this.Entity == null)
                throw new Exception("Entity does not exists");
            
            return this.Entity.Scene;
        }
    }
    /// <summary>
    /// An entity that is holding this component.
    /// </summary>
    public Entity Entity;
    /// <summary>
    /// A state if the component should update or draw.
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// An initialization of a component.
    /// </summary>
    public Component() {}

    /// <summary>
    /// A method that is called when the component is added on the entity.
    /// </summary>
    /// <param name="entity">An entity that will hold the component</param>
    public virtual void Added(Entity entity) 
    {
        ID = id++;
        Entity = entity;
        Active = true;
    }

    /// <summary>
    /// A method that is called if the entity entered the scene.
    /// </summary>
    /// <param name="scene">A scene that entity is entered on</param>
    public virtual void EntityEntered(Scene scene) 
    {
    }

    /// <summary>
    /// A method that is called if the entity exited the scene.
    /// </summary>
    /// <param name="scene">A scene that entity is previously on</param>
    public virtual void EntityExited(Scene scene) 
    {
    }
    
    /// <summary>
    /// A method that called per frame.
    /// </summary>
    /// <param name="delta">A delta time</param>
    public virtual void Update(double delta) {}
    /// <summary>
    /// A method that called per draw frame. 
    /// </summary>
    /// <param name="buffer">
    /// A <see cref="MoonWorks.Graphics.CommandBuffer"/> to send some command to the gpu
    /// </param>
    /// <param name="draw">
    /// A batching system that used to built a vertices or instances to be rendered later
    /// </param>
    public virtual void Draw(CommandBuffer buffer, DrawBatch draw) {}
    /// <summary>
    /// A method that is called when the entity removed this component.
    /// </summary>
    public virtual void Removed() 
    {
        Entity = null;
    }

    /// <summary>
    /// Component will detach itself from an entity. This still acts as if the entity removes
    /// this component.
    /// </summary>
    public void DetachSelf() 
    {
        Entity?.RemoveComponent(this);
    }

    /// <summary>
    /// Converts a component to string, this will return as [(Component Name) (ID)].
    /// </summary>
    /// <returns>A string with a format above</returns>
    public override string ToString()
    {
        var typeName = GetType().Name;
        return $"[{typeName} {ID}]";
    }

    /// <summary>
    /// Call this method to ensure that the entity is the correct type.
    /// </summary>
    /// <typeparam name="T">An entity type to ensure it is correct</typeparam>
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