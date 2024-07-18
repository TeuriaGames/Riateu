using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

/// <summary>
/// A vertex type to be used for the <see cref="GameContext.InstancedMaterial"/>.
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
    public Vector2 UV0 = uv[0];
    /// <summary>
    /// The second coordinates of a texture.
    /// </summary>
    public Vector2 UV1 = uv[1];
    /// <summary>
    /// The third coordinates of a texture.
    /// </summary>
    public Vector2 UV2 = uv[2];
    /// <summary>
    /// The fourth coordinates of a texture.
    /// </summary>
    public Vector2 UV3 = uv[3];
    /// <summary>
    /// The amount of pixel scale of the vertex.
    /// </summary>
    public Vector2 Scale = scale;
    /// <summary>
    /// A translation offset of the vertex.
    /// </summary>
    public Vector2 Origin;
    /// <summary>
    /// A rotation of the vertex.
    /// </summary>
    public float Rotation;
    /// <summary>
    /// A color that will be passed to the fragment shader.
    /// </summary>
    public Color Color = color;

    /// <summary>
    /// The element format used in the <see cref="GameContext.InstancedMaterial"/>. And also
    /// for other pipeline.
    /// </summary>
    public static VertexElementFormat[] Formats => [
        VertexElementFormat.Vector3,

        VertexElementFormat.Vector2,
        VertexElementFormat.Vector2,
        VertexElementFormat.Vector2,
        VertexElementFormat.Vector2,

        VertexElementFormat.Vector2,
        VertexElementFormat.Vector2,
        VertexElementFormat.Float,
        VertexElementFormat.Color,
    ];

    public static uint[] Offsets => [
        0, // 4x3=12

        12, // 2x4+12=20,
        20, // 2x4+20=28,
        28, // 2x4+28=36,
        36, // 2x4+36=44,

        44, // 2x4+44=52,
        52, // 2x4+52=60,
        60, // 4+60=64,
        64, // 4+64=68
    ];
}
