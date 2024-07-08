using System;
using System.Collections.Generic;

namespace Riateu.ECS;

public class World 
{
    internal Stack<uint> danglingID = new();
    internal static uint entityIDCount;
    private static uint componentIDCount;
    private static uint messageIDCount;

    internal Dictionary<Type, TypeID> ComponentIDs = new Dictionary<Type, TypeID>();
    internal Dictionary<Type, TypeID> MessageIDs = new Dictionary<Type, TypeID>();
    public List<ComponentList> ComponentLists = new List<ComponentList>();
    public List<List<ComponentQuery>> ComponentIDToQuery = new List<List<ComponentQuery>>();
    public List<HashSet<TypeID>> EntityComponentIndex = new List<HashSet<TypeID>>();

    public ComponentQueryBuilder QueryBuilder => new ComponentQueryBuilder(this);
    public Dictionary<ComponentQueryIndex, ComponentQuery> ComponentQueries = new Dictionary<ComponentQueryIndex, ComponentQuery>();

    public List<MessageList> MessageLists = new List<MessageList>();


    public EntityID CreateEntity() 
    {
        uint entityID;
        if (!danglingID.TryPop(out entityID)) 
        {
            entityID = World.entityIDCount++;
        }

        EntityComponentIndex.Add(new HashSet<TypeID>());
        EntityID entity = new EntityID(entityID);

        return entity;
    }

    public EntityID BuildEntity(EntityID entityID) 
    {
        return entityID;
    }

    public void Destroy(EntityID id) 
    {
        HashSet<TypeID> componentIDs = EntityComponentIndex[(int)id.id];
        foreach (var componentID in componentIDs) 
        {
            ComponentLists[(int)id.id].Remove(id);
        }
        EntityComponentIndex[(int)id.id].Clear();
        danglingID.Push(id.id);

        foreach (var query in ComponentIDToQuery[(int)id.id]) 
        {
            query.Remove(id);
        }
    }

    public ComponentList GetComponentList<T>(out TypeID outID) 
    where T : unmanaged
    {
        Type type = typeof(T);
        TypeID id = ComponentIDs[type];
        outID = id;
        return ComponentLists[(int)id.id];
    }

    public ComponentList GetComponentList<T>() 
    where T : unmanaged
    {
        Type type = typeof(T);
        TypeID id = ComponentIDs[type];
        return ComponentLists[(int)id.id];
    }

    public unsafe TypeID GetComponentID<T>() 
    where T : unmanaged
    {
        Type type = typeof(T);
        if (ComponentIDs.TryGetValue(type, out TypeID id)) 
        {
            return id;
        }

        TypeID componentID = new TypeID(componentIDCount++);
        ComponentIDs.Add(typeof(T), componentID);
        ComponentLists.Add(new ComponentList(sizeof(T)));
        ComponentIDToQuery.Add(new List<ComponentQuery>());
        return componentID;
    }

    public void AddComponent<T>(in EntityID entity, in T component) 
    where T : unmanaged
    {
        TypeID id = GetComponentID<T>();

        EntityComponentIndex[(int)entity.id].Add(id);
        ComponentLists[(int)id.id].Add<T>(entity, component);

        foreach (var query in ComponentIDToQuery[(int)id.id]) 
        {
            query.Update(entity);
        }
    }

    public ref T GetComponent<T>(in EntityID entity) 
    where T : unmanaged
    {
        return ref GetComponentList<T>().Get<T>(entity);
    }

    public bool RemoveComponent<T>(in EntityID entity) 
    where T : unmanaged
    {
        if (GetComponentList<T>(out TypeID id).Remove(entity)) 
        {
            foreach (var query in ComponentIDToQuery[(int)id.id]) 
            {
                query.Update(entity);
            }
            return true;
        }

        return false;
    }

    public bool HasComponent<T>(in EntityID entity) 
    where T : unmanaged
    {
        return GetComponentList<T>().Has<T>(entity);
    }

    public bool HasComponent(in EntityID entity, TypeID component) 
    {
        return EntityComponentIndex[(int)entity.id].Contains(component);
    }

    public ComponentQuery BuildQuery(ComponentQueryIndex queryIndex) 
    {
        if (!ComponentQueries.TryGetValue(queryIndex, out var query)) 
        {
            query = new ComponentQuery(this, queryIndex);

            foreach (TypeID include in queryIndex.Includes) 
            {
                ComponentIDToQuery[(int)include.id].Add(query);
            }

            foreach (TypeID exclude in queryIndex.Excludes) 
            {
                ComponentIDToQuery[(int)exclude.id].Add(query);
            }

            ComponentQueries.Add(queryIndex, query);
        }
        return query;
    }

    public unsafe TypeID GetMessageID<T>() 
    where T : unmanaged
    {
        Type type = typeof(T);
        if (MessageIDs.TryGetValue(type, out TypeID id)) 
        {
            return id;
        }

        TypeID messageID = new TypeID(messageIDCount++);
        MessageIDs.Add(typeof(T), messageID);
        MessageLists.Add(new MessageList(sizeof(T)));
        return messageID;
    }

    public MessageList GetMessageList<T>() 
    where T : unmanaged
    {
        TypeID id = GetMessageID<T>();
        return MessageLists[(int)id.id];
    }

    public void Send<T>(T message) 
    where T : unmanaged
    {
        MessageList list = GetMessageList<T>();
        list.Add(message);
    }

    public ReadOnlySpan<T> ReceiveAll<T>() 
    where T : unmanaged
    {
        MessageList list = GetMessageList<T>();
        return list.ReadAll<T>();
    }

    public T Receive<T>() 
    where T : unmanaged
    {
        MessageList list = GetMessageList<T>();
        return list.ReadFirst<T>();
    }

    public bool IsEmpty<T>() 
    where T : unmanaged
    {
        MessageList list = GetMessageList<T>();
        return list.IsEmpty();
    }

    public void Refresh() 
    {
        foreach (var list in MessageLists) 
        {
            list.Clear();
        }
    }
}