using System;
using System.Collections.Generic;
using MoonWorks.Graphics;
using Riateu.Components;

namespace Riateu;

/// <summary>
/// This class is a collections of game elements that you can fundamentally build your
/// game world here. The scene includes physics, game loop, and drawing. The scene can
/// holds entities here as it should be. Adding <see cref="Riateu.Entity"/> into scene will make the entity
/// process their initialization, update, and draw loop.
/// </summary>
public abstract class Scene
{
    /// <summary>
    /// The physics world of the scene.
    /// </summary>
    public World PhysicsWorld = new();

    /// <summary>
    /// The list of physics tags that uses for filtering the <see cref="Riateu.Entity"/> collision.
    /// </summary>
    public List<PhysicsComponent>[] SceneTags;

    /// <summary>
    /// The game application.
    /// </summary>
    public GameApp GameInstance { get; }
    /// <summary>
    /// A callback that is called when the entity is created.
    /// </summary>
    public Action<Entity> OnEntityCreated;
    /// <summary>
    /// A callback that is called when the entity is destroyed.
    /// </summary>
    public Action<Entity> OnEntityDestroyed;
    /// <summary>
    /// A state that can pause the update loop of some entities' <see cref="Riateu.Entity.PauseMode"/>
    /// inherited from the scene update loop.
    /// </summary>
    public bool Paused { get; set; }

    /// <summary>
    /// A collection of entities that are added.
    /// </summary>
    public Entities EntityList;

    /// <summary>
    /// An initialization for the scene.
    /// </summary>
    /// <param name="game">The game application</param>
    public Scene(GameApp game)
    {
        GameInstance = game;
        EntityList = new Entities(this);
        SceneTags = new List<PhysicsComponent>[Tag.TotalTags];
        for (int i = 0; i < SceneTags.Length; i++)
        {
            SceneTags[i] = new List<PhysicsComponent>();
        }
    }

    /// <summary>
    /// Add an <see cref="Riateu.Entity"/> to the scene.
    /// </summary>
    /// <param name="entity">The <see cref="Riateu.Entity"/> you want to add</param>
    public void Add(Entity entity)
    {
        EntityList.Add(entity);
    }

    /// <summary>
    /// Add the <see cref="Riateu.Components.PhysicsComponent"/> into the physics world.
    /// </summary>
    /// <param name="component">
    /// A <see cref="Riateu.Components.PhysicsComponent"/> will be added into the physics world
    /// </param>
    public void AddPhysics(PhysicsComponent component)
    {
        PhysicsWorld.Components.Add(component);
    }

    /// <summary>
    /// Remove the <see cref="Riateu.Components.PhysicsComponent"/> into the physics world.
    /// </summary>
    /// <param name="component">
    /// A <see cref="Riateu.Components.PhysicsComponent"/> will be removed into the physics world
    /// </param>
    public void RemovePhysics(PhysicsComponent component)
    {
        PhysicsWorld.Components.Remove(component);
    }

    /// <summary>
    /// Add a physics bit and component into the <see cref="Riateu.Scene.SceneTags"/>.
    /// </summary>
    /// <param name="component">
    /// A <see cref="Riateu.Components.PhysicsComponent"/> that will be added, alongside with
    /// their physics bit
    /// </param>
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

    /// <summary>
    /// Get a list of <see cref="Riateu.Components.PhysicsComponent"/> from a physics bits.
    /// </summary>
    /// <param name="tag">The tag of the physics</param>
    /// <returns>A list of <see cref="Riateu.Components.PhysicsComponent"/></returns>
    public List<PhysicsComponent> GetPhysicsFromBit(Tag tag)
    {
        return SceneTags[tag.ID];
    }

    /// <summary>
    /// Removes an <see cref="Riateu.Entity"/> from the scene.
    /// </summary>
    /// <param name="entity">An <see cref="Riateu.Entity"/> to remove</param>
    public void Remove(Entity entity)
    {
        EntityList.Remove(entity);
    }

    /// <summary>
    /// Remove a physics bit and component into the <see cref="Riateu.Scene.SceneTags"/>.
    /// </summary>
    /// <param name="component">
    /// A <see cref="Riateu.Components.PhysicsComponent"/> that will be added, alongside with
    /// their physics bit
    /// </param>
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

    /// <summary>
    /// Force sort all of the entities by <see cref="Riateu.Entity.Depth"/>.
    /// </summary>
    public void SortEntities()
    {
        EntityList.SortEntities();
    }

    internal void InternalUpdate(double delta)
    {
        EntityList.UpdateSystem();
        Update(delta);
        EntityList.Update(delta);
    }

    internal void InternalDraw(CommandBuffer buffer, Texture backbuffer)
    {
        Render(buffer, backbuffer);
    }

    /// <summary>
    /// Begin your scene initialization.
    /// </summary>
    public abstract void Begin();
    /// <summary>
    /// A method that runs on every update frame.
    /// </summary>
    /// <param name="delta">A delta time</param>
    public virtual void Update(double delta) {}

    /// <summary>
    /// A method that called during the draw loop. Do your draw calls here.
    /// </summary>
    /// <param name="buffer">A command buffer</param>
    /// <param name="backbuffer">The swapchain texture of the main window</param>
    public virtual void Render(CommandBuffer buffer, Texture backbuffer) {}

    /// <summary>
    /// End of the scene. Do your cleanup code here.
    /// </summary>
    public abstract void End();

    /// <summary>
    /// Get the <see cref="Riateu.GameApp"/> instance from a type
    /// </summary>
    /// <typeparam name="T">A type of the <see cref="Riateu.GameApp"/></typeparam>
    /// <returns>A <see cref="Riateu.GameApp"/></returns>
    public T GameApp<T>()
    where T : GameApp
    {
        return GameInstance as T;
    }
}
