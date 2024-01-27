using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu;

public enum PauseMode 
{
    Inherit,
    Single
}

public class Entity : IEnumerable<Component>
{
    private ulong InternalIDCount = 0;
    private List<Component> componentList = new List<Component>();
    public Scene Scene;
    public IReadOnlyList<Component> Components => componentList;
    public Transform Transform = new Transform();
    public Action OnRemoved;
    public bool Active;
    public string Name;
    public PauseMode PauseMode;

    public ulong NodeID;

    public Vector2 Position 
    {
        get => Transform.Position;
        set => Transform.Position = value;
    }
    public float PosX
    {
        get => Transform.PosX;
        set => Transform.PosX = value;
    }

    public float PosY
    {
        get => Transform.PosY;
        set => Transform.PosY = value;
    }

    public Vector2 LocalPosition 
    {
        get => Transform.LocalPosition;
        set => Transform.LocalPosition = value;
    }

    public float LocalPosX
    {
        get => Transform.LocalPosX;
        set => Transform.LocalPosX = value;
    }

    public float LocalPosY
    {
        get => Transform.LocalPosY;
        set => Transform.LocalPosY = value;
    }

    public float Rotation 
    {
        get => Transform.Rotation;
        set => Transform.Rotation = value;
    }

    public Vector2 Scale 
    {
        get => Transform.Scale;
        set => Transform.Scale = value;
    }
    public Color Modulate = Color.White;
    public float ZIndex;
    public int Depth;
    public int Tags;
    public bool Visible = true;

    public virtual void EnterScene(Scene scene) 
    {
        Active = true;
        foreach (var comp in componentList) 
            comp.EntityEntered(scene);
        Scene = scene;
        NodeID = InternalIDCount++;
    }
    public virtual void ExitScene(Scene scene) 
    {
        foreach (var comp in componentList) 
        {
            comp.Removed();
            comp.EntityExited(scene);
        }
        OnRemoved?.Invoke();
        Scene = null;
    }
    public virtual void Ready() {}
    public virtual void Update(double delta) 
    {
        for (int i = 0; i < componentList.Count; i++) 
        {
            if (!componentList[i].Active) continue;
            componentList[i].Update(delta);
        }
    }

    public virtual void Draw(CommandBuffer buffer, IBatch spriteBatch) 
    {
        if (!Visible) return;
        for (int i = 0; i < componentList.Count; i++) 
        {
            if (!componentList[i].Active) continue;
            componentList[i].Draw(buffer, spriteBatch);
        }
    }

    public void AddTransform(Entity entity, bool stay = false) 
    {
        entity.Transform.SetParent(Transform, stay);
    }

    public void AddTransform(Transform transform, bool stay = false) 
    {
        transform.SetParent(Transform, stay);
    }

    public void AddComponent(Component comp) 
    {
        componentList.Add(comp);
        comp.Added(this);
    }

    public void AddComponent<T>(T[] comps) 
    where T : Component
    {
        foreach (var comp in comps) 
        {
            AddComponent(comp);
        }
    }


    public T GetComponent<T>() where T : Component
    {
        Span<Component> comps = CollectionsMarshal.AsSpan(componentList);
        foreach (var comp in comps) 
        {
            if (comp is T c) 
            {
                return c;
            }
        }
    
        return default;
    }

    [Conditional("DEBUG")]
    public void AssertComponent<T>() where T : Component 
    {
        var component = GetComponent<T>();
        // SkyLog.Assert(component != null, $"This entity does not have {component} Component!");
    }

    public void RemoveComponent(Component comp) 
    {
        if (comp == null)
            return;
        comp.Removed();
        componentList.Remove(comp);
    }

    public void DestroySelf() 
    {
        Scene?.Remove(this);
    }

    public IEnumerator<Component> GetEnumerator() => componentList.GetEnumerator();
    

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}