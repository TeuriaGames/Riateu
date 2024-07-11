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
    public List<List<SearchResult>> ComponentIDToSearch = new List<List<SearchResult>>();
    public List<HashSet<TypeID>> EntityComponentIndex = new List<HashSet<TypeID>>();

    public SearchBuilder Search => new SearchBuilder(this);
    public Dictionary<SearchResultIndex, SearchResult> SearchResults = new Dictionary<SearchResultIndex, SearchResult>();

    public List<MessageList> MessageLists = new List<MessageList>();


    public EntityID CreateEntity() 
    {
        uint entityID;
        if (!danglingID.TryPop(out entityID)) 
        {
            entityID = World.entityIDCount++;
        }

        if (entityID == EntityComponentIndex.Count) 
        {
            EntityComponentIndex.Add(new HashSet<TypeID>());
        }

        EntityID entity = new EntityID(entityID);

        return entity;
    }

    public void Destroy(EntityID id) 
    {
        HashSet<TypeID> componentIDs = EntityComponentIndex[(int)id.id];
        foreach (var componentID in componentIDs) 
        {
            ComponentLists[(int)componentID.id].Remove(id);

            foreach (var query in ComponentIDToSearch[(int)componentID.id]) 
            {
                query.Remove(id);
            }
        }
        danglingID.Push(id.id);

        componentIDs.Clear();
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
        ComponentIDToSearch.Add(new List<SearchResult>());
        return componentID;
    }

    public void AddComponent<T>(in EntityID entity, in T component) 
    where T : unmanaged
    {
        TypeID id = GetComponentID<T>();

        if (!ComponentLists[(int)id.id].Add<T>(entity, component)) 
        {
            EntityComponentIndex[(int)entity.id].Add(id);

            foreach (var query in ComponentIDToSearch[(int)id.id]) 
            {
                query.Update(entity);
            }
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
            EntityComponentIndex[(int)entity.id].Remove(id);
            foreach (var query in ComponentIDToSearch[(int)id.id]) 
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

    public SearchResult ConfirmResult(SearchResultIndex queryIndex) 
    {
        if (!SearchResults.TryGetValue(queryIndex, out var query)) 
        {
            query = new SearchResult(this, queryIndex);

            foreach (TypeID include in queryIndex.Withs) 
            {
                ComponentIDToSearch[(int)include.id].Add(query);
            }

            foreach (TypeID exclude in queryIndex.Withouts) 
            {
                ComponentIDToSearch[(int)exclude.id].Add(query);
            }

            SearchResults.Add(queryIndex, query);
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

    public void SendMessage<T>(T message) 
    where T : unmanaged
    {
        MessageList list = GetMessageList<T>();
        list.Add(message);
    }

    public ReadOnlySpan<T> ReceiveAllMessage<T>() 
    where T : unmanaged
    {
        MessageList list = GetMessageList<T>();
        return list.ReadAll<T>();
    }

    public T ReceiveMessage<T>() 
    where T : unmanaged
    {
        MessageList list = GetMessageList<T>();
        return list.ReadFirst<T>();
    }

    public bool IsEmptyMessage<T>() 
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
