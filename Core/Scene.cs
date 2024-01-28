using System;
using System.Collections.Generic;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Components;
using Riateu.Graphics;

namespace Riateu;

public abstract class Scene 
{
    public World PhysicsWorld = new();

    public List<PhysicsComponent>[] SceneTags;

    public GameApp GameInstance;
    public Action<Entity> OnEntityCreated;
    public Action<Entity> OnEntityDeleted;
    public bool Paused { get; set; }

    public Entities EntityList;

    public Canvas SceneCanvas 
    {
        get => sceneCanvas;
        set 
        {
            sceneCanvas?.Dispose();
            sceneCanvas = value;
        }
    }
    private Canvas sceneCanvas;

    public Scene(GameApp game) :this(game, null)
    {
    }

    public Scene(GameApp game, Canvas canvas) 
    {
        GameInstance = game;
        EntityList = new Entities(this);
        sceneCanvas = canvas ?? Canvas.CreateDefault(this, game.GraphicsDevice);
        SceneTags = new List<PhysicsComponent>[Tag.TotalTags];
        for (int i = 0; i < SceneTags.Length; i++) 
        {
            SceneTags[i] = new List<PhysicsComponent>();
        }
    }

    public void Add(Entity entity) 
    {
        EntityList.Add(entity);
    }

    public void AddPhysics(PhysicsComponent component) 
    {
        PhysicsWorld.Components.Add(component);
    }

    public void RemovePhysics(PhysicsComponent component) 
    {
        PhysicsWorld.Components.Remove(component);
    }

    public void AddBit(PhysicsComponent component) 
    {
        if (component.Tags == -1) return;
        for (int i = 0; i < Tag.TotalTags; i++) 
        {
            if ((component.Tags & (1 << i)) != 0) 
            {
                SceneTags[i].Add(component);
            }
        }
    }

    public List<PhysicsComponent> GetPhysicsFromBit(Tag tag) 
    {
        return SceneTags[tag.ID];
    }

    public void Remove(Entity entity) 
    {
        EntityList.Remove(entity);
    }

    public void RemoveBit(PhysicsComponent component) 
    {
        if (component.Tags == -1) return;
        for (int i = 0; i < Tag.TotalTags; i++) 
        {
            if ((component.Tags & (1 << i)) != 0) 
            {
                SceneTags[i].Remove(component);
            }
        }
    }

    public void ApplyCurrentCanvasToBatch(IBatch batch, Sampler sampler) 
    {
        batch.Add(sceneCanvas.CanvasTexture, sampler, Vector2.Zero, Matrix3x2.Identity);
    }

    internal void InternalUpdate(double delta) 
    {
        EntityList.UpdateSystem();
        Update(delta);
        EntityList.Update(delta);
    }

    internal void InternalBeforeDraw(ref CommandBuffer buffer, IBatch batch) 
    {
        sceneCanvas.BeforeDraw(ref buffer, batch);
        BeforeDraw(ref buffer, batch);
    }

    internal void InternalDraw(CommandBuffer buffer, Texture backbuffer, IBatch batch) 
    {
        sceneCanvas.Draw(buffer, batch);
        Draw(buffer, backbuffer, batch);
    }

    internal void InternalAfterDraw(ref CommandBuffer buffer, IBatch batch) 
    {
        sceneCanvas.AfterDraw(ref buffer, batch);
        AfterDraw(ref buffer, batch);
    }

    public abstract void Begin();
    public virtual void Update(double delta) {}
    public virtual void BeforeDraw(ref CommandBuffer buffer, IBatch batch) {}
    public virtual void Draw(CommandBuffer buffer, Texture backbuffer, IBatch batch) {}
    public virtual void AfterDraw(ref CommandBuffer buffer, IBatch batch) {}
    public abstract void End();
}
