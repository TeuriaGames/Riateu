using System;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Components;
using Riateu.Graphics;

namespace Riateu;

public abstract class Scene 
{
    public World PhysicsWorld = new();

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
    }

    public void Add(Entity entity) 
    {
        EntityList.Add(entity);
    }

    public void AddPhysics(PhysicsComponent component) 
    {
        PhysicsWorld.Insert(component);
    }

    public void Remove(Entity entity) 
    {
        EntityList.Remove(entity);
    }

    public void RemovePhysics(PhysicsComponent component) 
    {
        PhysicsWorld.Remove(component);
    }

    public void ApplyCurrentCanvasToBatch(Batch batch, Sampler sampler) 
    {
        batch.Add(sceneCanvas.CanvasTexture, sampler, Vector2.Zero, Matrix3x2.Identity);
    }

    internal void InternalUpdate(double delta) 
    {
        EntityList.UpdateSystem();
        Update(delta);
        EntityList.Update(delta);
    }

    internal void InternalBeforeDraw(ref CommandBuffer buffer, Batch batch) 
    {
        sceneCanvas.BeforeDraw(ref buffer, batch);
        BeforeDraw(ref buffer, batch);
    }

    internal void InternalDraw(CommandBuffer buffer, Texture backbuffer, Batch batch) 
    {
        sceneCanvas.Draw(buffer, batch);
        Draw(buffer, backbuffer, batch);
    }

    internal void InternalAfterDraw(ref CommandBuffer buffer, Batch batch) 
    {
        sceneCanvas.AfterDraw(ref buffer, batch);
        AfterDraw(ref buffer, batch);
    }

    public abstract void Begin();
    public virtual void Update(double delta) {}
    public virtual void BeforeDraw(ref CommandBuffer buffer, Batch batch) {}
    public virtual void Draw(CommandBuffer buffer, Texture backbuffer, Batch batch) {}
    public virtual void AfterDraw(ref CommandBuffer buffer, Batch batch) {}
    public abstract void End();
}
