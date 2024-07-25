using System.Numerics;
using System.Runtime.InteropServices;

namespace Riateu.Graphics;

/// <summary>
/// A vertex type to be used for the shader info
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct PositionVertex(Vector3 position) : IVertexFormat
{
    /// <summary>
    /// The position of the vertex
    /// </summary>
    public Vector3 Position = position;

    public static VertexAttribute[] Attributes(uint binding)
    {
        return [
            new VertexAttribute(binding, 0, VertexElementFormat.Vector3, 0)
        ];
    }
}
