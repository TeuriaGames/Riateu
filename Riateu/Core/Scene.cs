using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Riateu.Components;
using Riateu.Physics;

namespace Riateu;

/// <summary>
/// This class is a collections of game elements that you can fundamentally build your
/// game world here. The scene includes physics, game loop, and drawing. The scene can
/// holds entities here as it should be. Adding <see cref="Riateu.Entity"/> into scene will make the entity
/// process their initialization, update, and draw loop.
/// </summary>
public abstract class Scene : GameLoop
{
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
    public Entities EntityList { get; set; }

    public SpatialHash SpatialHash { get; private set; }
    private List<PhysicsComponent> physicsColliders = new List<PhysicsComponent>();




    /// <summary>
    /// An initialization for the scene.
    /// </summary>
    /// <param name="game">The game application</param>
    public Scene(GameApp game) : base(game)
    {
        EntityList = new Entities(this);
        SpatialHash = new SpatialHash();
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
        physicsColliders.Add(component);
    }

    /// <summary>
    /// Remove the <see cref="Riateu.Components.PhysicsComponent"/> into the physics world.
    /// </summary>
    /// <param name="component">
    /// A <see cref="Riateu.Components.PhysicsComponent"/> will be removed into the physics world
    /// </param>
    public void RemovePhysics(PhysicsComponent component)
    {
        physicsColliders.Remove(component);
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
    /// Force sort all of the entities by <see cref="Riateu.Entity.Depth"/>.
    /// </summary>
    public void SortEntities()
    {
        EntityList.SortEntities();
    }

    /// <summary>
    /// A method that runs on every update frame and its sealed by default. Override the Process instead.
    /// </summary>
    /// <param name="delta"></param>
    public override sealed void Update(double delta) 
    {
        Span<PhysicsComponent> colliders = CollectionsMarshal.AsSpan(physicsColliders);
        for (int i = 0; i < colliders.Length; i++) 
        {
            SpatialHash.AddCollider(colliders[i]);
        }
        EntityList.UpdateSystem();
        Process(delta);
        EntityList.Update(delta);
        SpatialHash.Clear();
    }

    /// <summary>
    /// A method that runs on every update frame.
    /// </summary>
    /// <param name="delta">A delta time</param>
    public abstract void Process(double delta);

    /// <summary>
    /// Get the <see cref="Riateu.GameApp"/> instance from a type.
    /// </summary>
    /// <typeparam name="T">A type of the <see cref="Riateu.GameApp"/></typeparam>
    /// <returns>A <see cref="Riateu.GameApp"/></returns>
    public T GameApp<T>()
    where T : GameApp
    {
        return GameInstance as T;
    }
}
