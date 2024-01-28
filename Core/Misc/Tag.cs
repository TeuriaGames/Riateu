using System;
using System.Collections.Generic;

namespace Riateu;

public class Tag 
{
    internal static int TotalTags = 0;
    internal static Tag[] id = new Tag[32];
    private static Dictionary<string, Tag> name = new Dictionary<string, Tag>(StringComparer.OrdinalIgnoreCase);
    public int ID;
    public int Value;
    public string Name;

    public static Tag GetTag(string outputName) 
    {
        // SkyLog.Assert(name.ContainsKey(outputName), $"No tag with name '{outputName}' has been declared");

        return Tag.name[outputName];
    }

    public Tag(string outputName) 
    {
        // SkyLog.Assert(TotalTags < 32, "Maximum tag limit of 32 exceeded");
        // SkyLog.Assert(!name.ContainsKey(outputName), $"The tags with {outputName} has already existed!");   

        ID = TotalTags;
        Value = 1 << TotalTags;
        Name = outputName;

        id[ID] = this;
        name[outputName] = this;

        TotalTags++;
    }

    public static implicit operator int(Tag tag) => tag.Value;
}