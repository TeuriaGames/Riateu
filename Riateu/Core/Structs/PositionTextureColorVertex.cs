using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

/// <summary>
/// A vertex type to be used for the <see cref="GameContext.DefaultPipeline"/>. It can also
/// be used elsewhere.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct PositionTextureColorVertex(Vector3 position, Vector2 texCoord, Color color) : IVertexType
{
    /// <summary>
    /// The position of the vertex.
    /// </summary>
    public Vector3 Position = position;
    /// <summary>
    /// A screen-space location of the texture.
    /// </summary>
    public Vector2 TexCoord = texCoord;
    /// <summary>
    /// A color that will be passed to the fragment shader
    /// </summary>
    public Color Color = color;

    /// <summary>
    /// The element format used in the <see cref="GameContext.DefaultPipeline"/>. And also
    /// for other pipeline.
    /// </summary>
    public static VertexElementFormat[] Formats { get; } = [
        VertexElementFormat.Vector3,
        VertexElementFormat.Vector2,
        VertexElementFormat.Color,
    ];

    public static uint[] Offsets => [0, 12, 20];
}
