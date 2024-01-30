using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu.Components;

/// <summary>
/// A vertex type to be used for the <see cref="GameContext.InstancedPipeline"/>. 
/// It can also be used elsewhere.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct InstancedVertex(Vector3 position, Vector2 scale, UV uv, Color color) : IVertexType
{
    /// <summary>
    /// The position of the vertex.
    /// </summary>
    public Vector3 Position = position;
    /// <summary>
    /// The first coordinates of a texture.
    /// </summary>
    public Vector2 UV0 = uv.TopLeft;
    /// <summary>
    /// The second coordinates of a texture.
    /// </summary>
    public Vector2 UV1 = uv.BottomLeft;
    /// <summary>
    /// The third coordinates of a texture.
    /// </summary>
    public Vector2 UV2 = uv.TopRight;
    /// <summary>
    /// The fourth coordinates of a texture.
    /// </summary>
    public Vector2 UV3 = uv.BottomRight;
    /// <summary>
    /// The amount of pixel scale of the vertex.
    /// </summary>
    public Vector2 Scale = scale;
    /// <summary>
    /// A color that will be passed to the fragment shader 
    /// </summary>
    public Color Color = color;

    /// <summary>
    /// The element format used in the <see cref="GameContext.InstancedPipeline"/>. And also
    /// for other pipeline.
    /// </summary>
    public static VertexElementFormat[] Formats => [
        VertexElementFormat.Vector3,

        VertexElementFormat.Vector2,
        VertexElementFormat.Vector2,
        VertexElementFormat.Vector2,
        VertexElementFormat.Vector2,

        VertexElementFormat.Vector2,
        VertexElementFormat.Color,
    ];
}
