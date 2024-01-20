using System;
using System.Collections.Generic;
using MoonWorks;
using MoonWorks.Graphics;
using Riateu.Components;
using Riateu.Graphics;

namespace Riateu;

public abstract class Scene 
{
    private World physicsWorld = new();

    public GameApp GameInstance;
    public Action<Entity> OnEntityCreated;
    public Action<Entity> OnEntityDeleted;
    public bool Paused { get; set; }

    public Entities EntityList;

    public Canvas SceneCanvas;

    public Scene(GameApp game) :this(game, null)
    {
    }

    public Scene(GameApp game, Canvas canvas) 
    {
        GameInstance = game;
        EntityList = new Entities(this);
        SceneCanvas = canvas ?? Canvas.CreateDefault(this, game.GraphicsDevice);
    }

    public void Add(Entity entity) 
    {
        EntityList.Add(entity);
    }

    public void AddPhysics(PhysicsComponent component) 
    {
        physicsWorld.Insert(component);
        Logger.LogInfo("Added");
    }

    public void Remove(Entity entity) 
    {
        EntityList.Remove(entity);
    }

    public void RemovePhysics(PhysicsComponent component) 
    {
        physicsWorld.Remove(component);
    }

    internal void InternalUpdate(double delta) 
    {
        EntityList.UpdateSystem();
        foreach (var comp in physicsWorld.Components) 
        {
            foreach (var other in physicsWorld.Retrieve(comp)) 
            {
                comp.Check(other);
            }
        }
        Update(delta);
        EntityList.Update(delta);
    }

    internal void InternalBeforeDraw(ref CommandBuffer buffer, Batch batch) 
    {
        SceneCanvas.BeforeDraw(ref buffer, batch);
        BeforeDraw(ref buffer, batch);
    }

    internal void InternalDraw(ref CommandBuffer buffer, Texture backbuffer, Batch batch) 
    {
        SceneCanvas.Draw(ref buffer, batch);
        Draw(ref buffer, backbuffer, batch);
    }

    internal void InternalAfterDraw(ref CommandBuffer buffer, Batch batch) 
    {
        SceneCanvas.AfterDraw(ref buffer, batch);
        AfterDraw(ref buffer, batch);
    }

    public abstract void Begin();
    public virtual void Update(double delta) {}
    public virtual void BeforeDraw(ref CommandBuffer buffer, Batch batch) {}
    public virtual void Draw(ref CommandBuffer buffer, Texture backbuffer, Batch batch) {}
    public virtual void AfterDraw(ref CommandBuffer buffer, Batch batch) {}
    public abstract void End();
}
