using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct PositionTextureColorVertex(Vector3 position, Vector2 texCoord, Color color) : IVertexType
{
    public Vector3 Position = position;
    public Vector2 TexCoord = texCoord;
    public Color Color = color;

    public static VertexElementFormat[] Formats { get; } = [
        VertexElementFormat.Vector3,
        VertexElementFormat.Vector2,
        VertexElementFormat.Color,
    ];
}