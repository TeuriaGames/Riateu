using System.Numerics;
using System.Runtime.InteropServices;

namespace Riateu.Graphics;

/// <summary>
/// A vertex type to be used for the <see cref="GameContext.BatchMaterial"/>. It can also
/// be used elsewhere.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 48)]
public struct PositionTextureColorVertex(Vector3 position, Vector2 texCoord, Color color) : IVertexFormat
{
    /// <summary>
    /// The position of the vertex.
    /// </summary>
    [FieldOffset(0)]
    public Vector4 Position = new Vector4(position.X, position.Y, position.Z, 1.0f);
    /// <summary>
    /// A screen-space location of the texture.
    /// </summary>
    [FieldOffset(16)]
    public Vector2 TexCoord = texCoord;
    /// <summary>
    /// A color that will be passed to the fragment shader
    /// </summary>
    [FieldOffset(32)]
    public Vector4 Color = color.ToVector4();

    public static VertexAttribute[] Attributes(uint binding)
    {
        return [
            new VertexAttribute(binding, 0, VertexElementFormat.Float4, 0),
            new VertexAttribute(binding, 1, VertexElementFormat.Float2, 16),
            new VertexAttribute(binding, 2, VertexElementFormat.Float4, 32),
        ];
    }
}