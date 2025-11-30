using System.Numerics;
using System.Runtime.InteropServices;

namespace Riateu.Graphics;

/// <summary>
/// A vertex type to be used for the shader info
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct PositionColorVertex(Vector3 position, Color color) : IVertexFormat
{
    /// <summary>
    /// The position of the vertex
    /// </summary>
    public Vector4 Position = new Vector4(position.X, position.Y, position.Z, 1);
    /// <summary>
    /// A color that will be passed to the fragment shader
    /// </summary>
    public Color Color = color;


    public static VertexAttribute[] Attributes(uint binding)
    {
        return [
            new VertexAttribute(binding, 0, VertexElementFormat.Float3, 0),
            new VertexAttribute(binding, 0, VertexElementFormat.Ubyte4Norm, 16),
        ];
    }
}
