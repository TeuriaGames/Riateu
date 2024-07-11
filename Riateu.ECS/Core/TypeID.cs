namespace Riateu.ECS;

public record struct TypeID(uint id);


internal class ComponentTypeIdAssigner 
{
    protected static uint Counter;
}

internal class ComponentTypeIdAssigner<T> : ComponentTypeIdAssigner 
where T : unmanaged
{
    public static readonly uint Id;

    static ComponentTypeIdAssigner() 
    {
        Id = Counter++;
    }
}


internal class MessageTypeIdAssigner
{
    protected static uint Counter;
}

internal class MessageTypeIdAssigner<T> : MessageTypeIdAssigner
where T : unmanaged
{
    public static readonly uint Id;

    static MessageTypeIdAssigner() 
    {
        Id = Counter++;
    }
}


internal class RelationTypeIdAssigner
{
    protected static uint Counter;
}

internal class RelationTypeIdAssigner<T> : RelationTypeIdAssigner
where T : unmanaged
{
    public static readonly uint Id;

    static RelationTypeIdAssigner() 
    {
        Id = Counter++;
    }
}

