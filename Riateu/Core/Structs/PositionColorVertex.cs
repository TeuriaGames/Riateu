using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

/// <summary>
/// A vertex type to be used for the shader info
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct PositionColorVertex(Vector3 position, Color color) : IVertexType
{
    /// <summary>
    /// The position of the vertex
    /// </summary>
    public Vector3 Position = position;
    /// <summary>
    /// A color that will be passed to the fragment shader
    /// </summary>
    public Color Color = color;

    /// <summary>
    /// The element format used for the graphics pipeline.
    /// </summary>
    public static VertexElementFormat[] Formats { get; } = new VertexElementFormat[2]
    {
        VertexElementFormat.Vector3,
        VertexElementFormat.Color
    };

    public static uint[] Offsets => [0, 12];
}
