using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

/// <summary>
/// A vertex type to be used for the <see cref="GameContext.DefaultPipeline"/>. It can also
/// be used elsewhere.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 48)]
public struct PositionTextureColorVertex(Vector3 position, Vector2 texCoord, Color color) : IVertexType
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

    /// <summary>
    /// The element format used in the <see cref="GameContext.DefaultPipeline"/>. And also
    /// for other pipeline.
    /// </summary>
    public static VertexElementFormat[] Formats { get; } = [
        VertexElementFormat.Vector4,
        VertexElementFormat.Vector2,
        VertexElementFormat.Vector4,
    ];

    public static uint[] Offsets => [0, 16, 32];
}
