using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct PositionColorVertex(Vector3 position, Color color) : IVertexType
{
    public Vector3 Position = position;
    public Color Color = color;

    public static VertexElementFormat[] Formats { get; } = new VertexElementFormat[2] 
    {
        VertexElementFormat.Vector3,
        VertexElementFormat.Color
    };
}
