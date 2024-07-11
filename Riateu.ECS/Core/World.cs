using System;
using System.Collections.Generic;

namespace Riateu.ECS;

public class RelationList : IDisposable
{
    private AnyOpaqueArray relations;
    private AnyOpaqueArray relationDatas;
    private int elementSize;

    private Dictionary<(EntityID, EntityID), int> Mappings = new Dictionary<(EntityID, EntityID), int>();
    private Dictionary<EntityID, WeakList<EntityID>> InEntityRelations = new Dictionary<EntityID, WeakList<EntityID>>();
    private Dictionary<EntityID, WeakList<EntityID>> OutEntityRelations = new Dictionary<EntityID, WeakList<EntityID>>();
    private Stack<WeakList<EntityID>> setPool = new Stack<WeakList<EntityID>>();
    private bool disposedValue;

    public unsafe RelationList(int elementSize) 
    {
        this.elementSize = elementSize;
        relations = new AnyOpaqueArray(sizeof((EntityID, EntityID)));
        relationDatas = new AnyOpaqueArray(elementSize);
    }

    public void Set<T>(in EntityID a, in EntityID b, in T data) 
    where T : unmanaged
    {
        var relationPair = (a, b);

        if (Mappings.TryGetValue(relationPair, out int val)) 
        {
            relationDatas.Set(val, data);
            return;
        }

        if (!OutEntityRelations.ContainsKey(a)) 
        {
            OutEntityRelations[a] = CreateList();
        }
        OutEntityRelations[a].Add(b);

        if (!InEntityRelations.ContainsKey(b)) 
        {
            InEntityRelations[b] = CreateList();
        }
        InEntityRelations[b].Add(a);

        relations.Add(relationPair);
        relationDatas.Add(data);
        Mappings.Add(relationPair, relations.Count - 1);
    }

    public ref T Get<T>(in EntityID a, in EntityID b) 
    where T : unmanaged
    {
        int relationIndex = Mappings[(a, b)];
        return ref relationDatas.Get<T>(relationIndex);
    }

    public bool Has(in EntityID a, in EntityID b) 
    {
        return Mappings.ContainsKey((a, b));
    }

    public (bool, bool) Remove(in EntityID a, in EntityID b) 
    {
        bool emptyA = false;
        bool emptyB = false;
        var relationPair = (a, b);

        if (OutEntityRelations.TryGetValue(a, out var outRelation)) 
        {
            outRelation.Remove(b);
            if (OutEntityRelations[a].Count == 0) 
            {
                emptyA = true;
            }
        }
        if (InEntityRelations.TryGetValue(b, out var inRelation)) 
        {
            inRelation.Remove(a);
            if (InEntityRelations[b].Count == 0) 
            {
                emptyB = true;
            }
        }

        if (Mappings.TryGetValue(relationPair, out var index)) 
        {
            var lastElementIndex = relations.Count - 1;

            if (index != lastElementIndex) 
            {
                var lastRelation = relations.Get<(EntityID, EntityID)>(lastElementIndex);
                Mappings[lastRelation] = index;
            }

            relationDatas.Remove(index);
            relations.Remove(index);

            Mappings.Remove(relationPair);
        }

        return (emptyA, emptyB);
    }

    public WeakEnumerator<EntityID> InAllRelations(in EntityID entityID) 
    {
        if (InEntityRelations.TryGetValue(entityID, out WeakList<EntityID> entityRelation)) 
        {
            return entityRelation.GetEnumerator();
        }

        return WeakEnumerator<EntityID>.Empty;
    }

    private WeakList<EntityID> CreateList() 
    {
        if (setPool.Count == 0) 
        {
            setPool.Push(new WeakList<EntityID>());
        }

        return setPool.Pop();
    }

    private void DestroyList(WeakList<EntityID> list) 
    {
        list.Clear();
        setPool.Push(list);
    }

    public void Clear() 
    {
        Mappings.Clear();

        relations.Clear();
        relationDatas.Clear();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing) 
            {
                relations.Dispose();
                relationDatas.Dispose();
            }

            disposedValue = true;
        }
    }

    ~RelationList()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public class World 
{
    internal Stack<uint> danglingID = new();
    internal static uint entityIDCount;

    public List<ComponentList> ComponentLists = new List<ComponentList>();
    public List<List<SearchResult>> ComponentIDToSearch = new List<List<SearchResult>>();
    public List<HashSet<TypeID>> EntityComponentIndex = new List<HashSet<TypeID>>();

    public SearchBuilder Search => new SearchBuilder(this);
    public Dictionary<SearchResultIndex, SearchResult> SearchResults = new Dictionary<SearchResultIndex, SearchResult>();

    public List<MessageList> MessageLists = new List<MessageList>();
    public List<RelationList> RelationLists = new List<RelationList>();


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
        TypeID id = GetComponentID<T>();
        outID = id;
        return ComponentLists[(int)id.id];
    }

    public ComponentList GetComponentList<T>() 
    where T : unmanaged
    {
        TypeID id = GetComponentID<T>();
        return ComponentLists[(int)id.id];
    }

    public unsafe TypeID GetComponentID<T>() 
    where T : unmanaged
    {
        TypeID componentID = new TypeID(ComponentTypeIdAssigner<T>.Id);
        if (componentID.id < ComponentLists.Count) 
        {
            return componentID;
        }

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
        TypeID messageID = new TypeID(MessageTypeIdAssigner<T>.Id);
        if (messageID.id < MessageLists.Count) 
        {
            return messageID;
        }

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

    public unsafe TypeID GetRelationID<T>() 
    where T : unmanaged
    {
        TypeID relationID = new TypeID(RelationTypeIdAssigner<T>.Id);
        if (relationID.id < RelationLists.Count) 
        {
            return relationID;
        }

        MessageLists.Add(new MessageList(sizeof(T)));
        return relationID;
    }

    public RelationList GetRelationList<T>() 
    where T : unmanaged
    {
        TypeID relationID = GetRelationID<T>();
        return RelationLists[(int)relationID.id];
    }

    public void Relate<T>(in EntityID a, in EntityID b, in T data) 
    where T : unmanaged
    {
        RelationList list = GetRelationList<T>();
        list.Set(a, b, data);
    }

    public void Unrelate<T>(in EntityID a, in EntityID b)
    where T : unmanaged 
    {
        RelationList list = GetRelationList<T>();
    }

    public void Refresh() 
    {
        foreach (var list in MessageLists) 
        {
            list.Clear();
        }
    }
}
