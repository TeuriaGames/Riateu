using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoonWorks.Graphics;
using Riateu.Graphics;

namespace Riateu;

public sealed class Entities : IEnumerable<Entity>
{
    public Scene Scene { get; private set; }

    private WeakList<Entity> entities = new WeakList<Entity>();
    private List<Entity> add = new List<Entity>();
    private List<Entity> remove = new List<Entity>();
    private List<Entity> entering = new List<Entity>();
    private HashSet<Entity> current = new HashSet<Entity>();
    private HashSet<Entity> adding = new HashSet<Entity>();
    private bool unsorted;

    public int Count => entities.Count;
    internal Entities(Scene scene) 
    {
        Scene = scene;
    }

    public Entity this[int idx] 
    {
        get 
        {
#if DEBUG
            if (idx < 0 || idx >= entities.Count) { throw new IndexOutOfRangeException(); }
#endif
            return entities[idx];
        }    
    }

    public void UpdateSystem() 
    {
        if (add.Count > 0) 
        {
            for (int i = 0; i < add.Count; i++) 
            {
                var entity = add[i];
                if (!current.Contains(entity)) 
                {
                    current.Add(entity);
                    entities.Add(entity);
                    if (Scene == null) { continue; }
                    entity.EnterScene(Scene);
                    Scene.OnEntityCreated?.Invoke(entity);
                }
            }
            unsorted = true;
        }

        if (remove.Count > 0) 
        {
            for (int i = 0; i < remove.Count; i++) 
            {
                var entity = remove[i];
                if (entities.Contains(entity)) 
                {
                    current.Remove(entity);
                    entities.Remove(entity);

                    if (Scene == null) { continue; }
                    entity.ExitScene(Scene);
                    Scene.OnEntityDeleted?.Invoke(entity);
                }
            }
            remove.Clear();
        }

        if (unsorted) 
        {
            unsorted = false;
            entities.Sort(CompareDepth);
        }

        if (add.Count > 0) 
        {
            entering.AddRange(add);
            add.Clear();
            adding.Clear();
            foreach (var entity in entering) 
            {
                if (entity.Scene == Scene) 
                {
                    entity.Ready();
                }
            }
            entering.Clear();
        }
    }

    internal void SortEntities() 
    {
        unsorted = true;
    }

    public void Add(Entity entity) 
    {
        if (!adding.Contains(entity) && !current.Contains(entity)) 
        {
            add.Add(entity);
            adding.Add(entity);
        }
    }

    public void Remove(Entity entity) 
    {
        remove.Add(entity);
    }

    public void Clear() 
    {
        for (int i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];
            remove.Add(entity);
        }
    }

    internal void Update(double delta) 
    {
        for (int i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];
            if (!entity.Active)
                continue;
            
            if (Scene.Paused && entity.PauseMode != PauseMode.Single)
                continue;

            entity.Update(delta);
        }
    }

    public void Draw(CommandBuffer buffer, IBatch spriteBatch) 
    {
        for (int i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];
            if (entity.Visible) { entity.Draw(buffer, spriteBatch); }
        }
    }

    public List<Entity> GetEntitiesAndSortedByID() 
    {
        var sortedEntities = entities.ToList();
        sortedEntities.Sort(CompareID);
        return sortedEntities;
    }


    public List<T> GetEntities<T>() 
    where T : Entity
    {
        var list = Enumerable.Empty<T>().ToList();
        for (int i = 0; i < entities.Count; i++) 
        {
            var entity = entities[i];
            if (entity is T ent) 
                list.Add(ent);
        }

        foreach (var entity in adding) 
        {
            if (entity is T ent) 
                list.Add(ent);
        }
        return list;
    }

    public List<Entity> GetEntities(string name) 
    {
        var list = new List<Entity>(); 
        for (int i = 0; i < entities.Count; i++) 
        {
            var entity = entities[i];
            if (entity.Name == name)
            {
                list.Add(entity);
            }
        }

        foreach (var entity in adding) 
        {
            if (entity.Name == name)
            {
                list.Add(entity);
            }
        }
        return list;
    }

    public T GetEntity<T>() 
    where T : Entity
    {
        for (int i = 0; i < entities.Count; i++) 
        {
            var entity = entities[i];
            if (entity is T ent) 
                return ent;
        }

        foreach (var entity in adding) 
        {
            if (entity is T ent) 
                return ent;
        }
        return null;
    }

    public T GetEntity<T>(string name) 
    where T : Entity
    {
        for (int i = 0; i < entities.Count; i++) 
        {
            var entity = entities[i];
            if (entity.Name == name)
            {
                return entity as T;
            }
        }

        foreach (var entity in adding) 
        {
            if (entity.Name == name)
            {
                return entity as T;
            }
        }
        return null;
    }

    public IEnumerator<Entity> GetEnumerator()
    {
        for (int i = 0; i < entities.Count; i++)
            yield return entities[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static Comparison<Entity> CompareDepth = (a, b) => Math.Sign(b.Depth - a.Depth);
    public static Comparison<Entity> CompareID = (a, b) => 
    {
        var aN = a.NodeID;
        var bN = b.NodeID;
        if (aN > bN)
            return 1;
        if (aN < bN)
            return -1;
        return 0;
    };
}