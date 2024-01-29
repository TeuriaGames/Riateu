using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu.Components;

[StructLayout(LayoutKind.Sequential)]
public struct InstancedVertex(Vector3 position, Vector2 scale, UV uv, Color color) : IVertexType
{
    public Vector3 Position = position;
    public Vector2 UV0 = uv.TopLeft;
    public Vector2 UV1 = uv.BottomLeft;
    public Vector2 UV2 = uv.TopRight;
    public Vector2 UV3 = uv.BottomRight;
    public Vector2 Scale = scale;
    public Color Color = color;

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