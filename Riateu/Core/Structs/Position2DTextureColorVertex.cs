using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

/// <summary>
/// A vertex type to be used for the shader info. Usually used by the ImGui renderer. 
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Position2DTextureColorVertex : IVertexType
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
    public Position2DTextureColorVertex(
        Vector2 position,
        Vector2 texcoord,
        Color color
    )
    {
        Position = position;
        TexCoord = texcoord;
        Color = color;
    }

    /// <summary>
    /// The element format used for the graphics pipeline.
    /// </summary>
    public static VertexElementFormat[] Formats { get; } = new VertexElementFormat[3]
    {
        VertexElementFormat.Vector2,
        VertexElementFormat.Vector2,
        VertexElementFormat.Color
    };
}