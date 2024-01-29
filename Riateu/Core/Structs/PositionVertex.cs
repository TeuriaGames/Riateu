using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct PositionVertex(Vector3 position) : IVertexType
{
    public Vector3 Position = position;

    public static VertexElementFormat[] Formats { get; } =  
    [
        VertexElementFormat.Vector3
    ];
}
