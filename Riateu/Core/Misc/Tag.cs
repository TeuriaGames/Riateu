using System;
using System.Collections.Generic;

namespace Riateu;

/// <summary>
/// This class contains a tag value that can be used for filtering. The maximum limit of 
/// the tags is 64.
/// </summary>
public class Tag 
{
    internal static ulong TotalTags = 0;
    internal static Tag[] id = new Tag[64];
    private static Dictionary<string, Tag> name = new Dictionary<string, Tag>(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// An id of the <see cref="Riateu.Tag"/>.
    /// </summary>
    public int ID;
    /// <summary>
    /// The bit value. 
    /// </summary>
    public ulong Value;
    /// <summary>
    /// The name of the <see cref="Riateu.Tag"/>.
    /// </summary>
    public string Name;

    /// <summary>
    /// Get a <see cref="Riateu.Tag"/> from the name.
    /// </summary>
    /// <param name="outputName">The name of the <see cref="Riateu.Tag"/></param>
    /// <returns>A <see cref="Riateu.Tag"/></returns>
    public static Tag GetTag(string outputName) 
    {
        // SkyLog.Assert(name.ContainsKey(outputName), $"No tag with name '{outputName}' has been declared");

        return Tag.name[outputName];
    }

    /// <summary>
    /// Initialize a <see cref="Riateu.Tag"/>. It will increase its maximum index.
    /// </summary>
    /// <param name="outputName">The name of the <see cref="Riateu.Tag"/></param>
    public Tag(string outputName) 
    {
#if DEBUG
        if (TotalTags == 64) 
        {
            throw new Exception("Maximum tag limit of 64 exceeded!");
        }
        if (name.ContainsKey(outputName)) 
        {
            throw new Exception($"The tags with {outputName} has already existed!");
        }
#endif

        ID = (int)TotalTags;
        Value = (ulong)1 << (int)TotalTags;
        Name = outputName;

        id[ID] = this;
        name[outputName] = this;

        TotalTags++;
    }

    /// <summary>
    /// Implicitly known tag as int.
    /// </summary>
    /// <param name="tag">A tag to access its <see cref="Riateu.Tag.Value"/></param>
    public static implicit operator ulong(Tag tag) => tag.Value;
}