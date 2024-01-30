using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

/// <summary>
/// A vertex type to be used for the shader info
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct PositionVertex(Vector3 position) : IVertexType
{
    /// <summary>
    /// The position of the vertex
    /// </summary>
    public Vector3 Position = position;

    /// <summary>
    /// The element format used for the graphics pipeline.
    /// </summary>
    public static VertexElementFormat[] Formats { get; } =  
    [
        VertexElementFormat.Vector3
    ];
}
