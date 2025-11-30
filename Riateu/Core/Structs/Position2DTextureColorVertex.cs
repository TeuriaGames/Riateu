using System.Numerics;
using System.Runtime.InteropServices;

namespace Riateu.Graphics;

/// <summary>
/// A vertex type to be used for the shader info. Usually used by the ImGui renderer.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Position2DTextureColorVertex : IVertexFormat
{
    /// <summary>
    /// The position of the vertex in 2d space.
    /// </summary>
    public Vector2 Position;
    /// <summary>
    /// A screen-space location of the texture.
    /// </summary>
    public Vector2 TexCoord;
    /// <summary>
    /// A color that will be passed to the fragment shader
    /// </summary>
    public Color Color;

    /// <summary>
    /// Initialization for this struct.
    /// </summary>
    /// <param name="position">The position of the vertex in 2d space</param>
    /// <param name="texcoord">A screen-space location of the texture</param>
    /// <param name="color">A color that will be passed to the fragment shader</param>
    public Position2DTextureColorVertex(Vector2 position, Vector2 texcoord, Color color)
    {
        Position = position;
        TexCoord = texcoord;
        Color = color;
    }

    public static VertexAttribute[] Attributes(uint binding)
    {
        return [
            new VertexAttribute(binding, 0, VertexElementFormat.Float2, 0),
            new VertexAttribute(binding, 1, VertexElementFormat.Float2, 8),
            new VertexAttribute(binding, 2, VertexElementFormat.Ubyte4Norm, 16),
        ];
    }
}
