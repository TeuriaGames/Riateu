using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu;

/// <summary>
/// An enum specifically for entities, tells to whether it should pause alongside of the scene 
/// </summary>
public enum PauseMode 
{
    /// <summary>
    /// Pause the entity if the scene is paused.
    /// </summary>
    Inherit,
    /// <summary>
    /// Do not pause the entity if the scene is paused.
    /// </summary>
    Single
}

/// <summary>
/// An object that can be added into the scene. Entity should holds component that defy its
/// behavior. Entity can also have their own behavior and it should manage how the component
/// is composed.
/// </summary>
public class Entity : IEnumerable<Component>
{
    private ulong InternalIDCount = 0;
    private List<Component> componentList = new List<Component>();
    /// <summary>
    /// The scene that is entity in.
    /// </summary>
    public Scene Scene;
    /// <summary>
    /// A list of components that the entity has.
    /// </summary>
    public IReadOnlyList<Component> Components => componentList;
    /// <summary>
    /// The main transform of an entity.
    /// </summary>
    public Transform Transform = new Transform();
    /// <summary>
    /// A callback that is called when the entity is removed.
    /// </summary>
    public Action OnRemoved;
    /// <summary>
    /// A state that tells whether the entity should update.
    /// </summary>
    public bool Active;
    /// <summary>
    /// The name or id of the entity.
    /// </summary>
    public string Name;
    /// <summary>
    /// The pause mode whether it should pause alongside of the scene.
    /// </summary>
    public PauseMode PauseMode;

    /// <summary>
    /// The id of the entity.
    /// </summary>
    public ulong NodeID;

    /// <summary>
    /// The global transform position of the entity.
    /// </summary>
    public Vector2 Position 
    {
        get => Transform.Position;
        set => Transform.Position = value;
    }

    /// <summary>
    /// The global x-axis of the entity.
    /// </summary>
    public float PosX
    {
        get => Transform.PosX;
        set => Transform.PosX = value;
    }

    /// <summary>
    /// The global y-axis of the entity.
    /// </summary>
    public float PosY
    {
        get => Transform.PosY;
        set => Transform.PosY = value;
    }

    /// <summary>
    /// The local transform position of the entity.
    /// </summary>
    public Vector2 LocalPosition 
    {
        get => Transform.LocalPosition;
        set => Transform.LocalPosition = value;
    }

    /// <summary>
    /// The local x-axis of the entity.
    /// </summary>
    public float LocalPosX
    {
        get => Transform.LocalPosX;
        set => Transform.LocalPosX = value;
    }

    /// <summary>
    /// The local y-axis of the entity.
    /// </summary>
    public float LocalPosY
    {
        get => Transform.LocalPosY;
        set => Transform.LocalPosY = value;
    }

    /// <summary>
    /// The rotation radians of the entity.
    /// </summary>
    public float Rotation 
    {
        get => Transform.Rotation;
        set => Transform.Rotation = value;
    }

    /// <summary>
    /// The scale of the entity.
    /// </summary>
    public Vector2 Scale 
    {
        get => Transform.Scale;
        set => Transform.Scale = value;
    }

    /// <summary>
    /// The material color of the entity.
    /// </summary>
    public Color Modulate = Color.White;
    /// <summary>
    /// The Z-Depth of the entity.
    /// </summary>
    public int Depth;

    /// <summary>
    /// An entity state whether it should draw on the draw loop.
    /// </summary>
    public bool Visible = true;

    /// <summary>
    /// Called when the entity has entered the scene. 
    /// </summary>
    /// <param name="scene">A scene that entity has entered to</param>
    public virtual void EnterScene(Scene scene) 
    {
        Active = true;
        Scene = scene;
        foreach (var comp in componentList) 
            comp.EntityEntered(scene);
        NodeID = InternalIDCount++;
    }

    /// <summary>
    /// Called when the entity has exited the scene. 
    /// </summary>
    /// <param name="scene">A scene that entity is previously on</param>
    public virtual void ExitScene(Scene scene) 
    {
        var removing = new List<Component>();
        foreach (var comp in componentList) 
        {
            comp.EntityExited(scene);
            removing.Add(comp);
        }
        foreach (var targetComponent in removing) 
        {
            RemoveComponent(targetComponent);
        }
        OnRemoved?.Invoke();
        Scene = null;
    }
    
    /// <summary>
    /// Called when the entity is ready.
    /// </summary>
    public virtual void Ready() {}
    
    /// <summary>
    /// Called every update frame.
    /// </summary>
    /// <param name="delta">A delta time</param>
    public virtual void Update(double delta) 
    {
        for (int i = 0; i < componentList.Count; i++) 
        {
            if (!componentList[i].Active) continue;
            componentList[i].Update(delta);
        }
    }

    /// <summary>
    /// Called every draw frame.
    /// </summary>
    /// <param name="draw">
    /// A batching system that used to built a vertices or instances to be rendered later
    /// </param>
    public virtual void Draw(Batch draw) 
    {
        if (!Visible) return;
        for (int i = 0; i < componentList.Count; i++) 
        {
            if (!componentList[i].Active) continue;
            componentList[i].Draw(draw);
        }
    }

    /// <summary>
    /// Add a child transform from an entity's transform.
    /// </summary>
    /// <param name="entity">
    /// An entity to access their transform to become child with this entity's transform
    /// </param>
    /// <param name="stay">Whether the transform state should stay</param>
    public void AddTransform(Entity entity, bool stay = false) 
    {
        entity.Transform.SetParent(Transform, stay);
    }

    /// <summary>
    /// Add a child transform from an entity.
    /// </summary>
    /// <param name="transform">A trasnform to become child with this entity's transform</param>
    /// <param name="stay">Whether the transform state should stay</param>
    public void AddTransform(Transform transform, bool stay = false) 
    {
        transform.SetParent(Transform, stay);
    }

    /// <summary>
    /// Add a component to the entity.
    /// </summary>
    /// <param name="comp">A component to be added in this entity</param>
    public void AddComponent(Component comp) 
    {
        componentList.Add(comp);
        comp.Added(this);
    }

    /// <summary>
    /// Add an array of components to the entity.
    /// </summary>
    /// <param name="comps">An array of components to be added in this entity</param>
    /// <typeparam name="T">A type of the component</typeparam>
    public void AddComponent<T>(T[] comps) 
    where T : Component
    {
        foreach (var comp in comps) 
        {
            AddComponent(comp);
        }
    }

    /// <summary>
    /// Get a component from a type.
    /// </summary>
    /// <typeparam name="T">A type of the component to get with</typeparam>
    /// <returns>A first occurrence of a component from this entity</returns>
    public T GetComponent<T>() where T : Component
    {
        Span<Component> comps = CollectionsMarshal.AsSpan(componentList);
        ref var componentSearch = ref MemoryMarshal.GetReference(comps);
        for (int i = 0; i < comps.Length; i++)
        {
            var item = Unsafe.Add(ref componentSearch, i);
            if (item is T c) 
            {
                return c;
            }
        }
    
        return default;
    }

    /// <summary>
    /// Assert a component whether it exists in this entity or not.
    /// </summary>
    /// <typeparam name="T">A type of a component to assert with</typeparam>
    [Conditional("DEBUG")]
    public void AssertComponent<T>() where T : Component 
    {
        var component = GetComponent<T>();
        // SkyLog.Assert(component != null, $"This entity does not have {component} Component!");
    }

    /// <summary>
    /// Removes a component from this entity
    /// </summary>
    /// <param name="comp">A component to be removed in this entity</param>
    public void RemoveComponent(Component comp) 
    {
        if (comp == null)
            return;
        comp.Removed();
        componentList.Remove(comp);
    }

    /// <summary>
    /// Removes itself from a scene. It acts like the scene removes the entity. 
    /// </summary>
    public void DestroySelf() 
    {
        Scene?.Remove(this);
    }

    /// <summary>
    /// Get an enumerator from a component list.
    /// </summary>
    /// <returns>An <see cref="System.Collections.Generic.IEnumerable{T}"/> component</returns>
    public IEnumerator<Component> GetEnumerator() => componentList.GetEnumerator();
    

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Get a local position from a target.
    /// </summary>
    /// <param name="target">A target to calculate its relativity from the Entity's position</param>
    /// <returns>A localized position of this entity</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToLocal(Vector2 target) 
    {
        return new Vector2(target.X - PosX, target.Y - PosY);
    }
}