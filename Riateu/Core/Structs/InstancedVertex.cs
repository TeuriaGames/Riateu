using System.Numerics;
using System.Runtime.InteropServices;

namespace Riateu.Graphics;

/// <summary>
/// A vertex type to be used for any instancing material.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct InstancedVertex(Vector3 position, Vector2 scale, UV uv, Color color) : IVertexFormat
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


    public static VertexAttribute[] Attributes(uint binding)
    {
        return [
            new VertexAttribute(binding, 0, VertexElementFormat.Float3, 0),

            new VertexAttribute(binding, 1, VertexElementFormat.Float2, 12),
            new VertexAttribute(binding, 2, VertexElementFormat.Float2, 20),
            new VertexAttribute(binding, 3, VertexElementFormat.Float2, 28),
            new VertexAttribute(binding, 4, VertexElementFormat.Float2, 36),

            new VertexAttribute(binding, 5, VertexElementFormat.Float2, 44),
            new VertexAttribute(binding, 6, VertexElementFormat.Float2, 52),
            new VertexAttribute(binding, 7, VertexElementFormat.Float, 60),
            new VertexAttribute(binding, 8, VertexElementFormat.Ubyte4Norm, 64),
        ];
    }
}
