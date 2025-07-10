using System.Numerics;
using System.Runtime.InteropServices;

namespace LiteSkinViewer3D.OpenGL;

/// <summary>
/// OpenGL顶点结构：包含位置、UV坐标与法线，用于传给GPU
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct VertexDataGL {
    public Vector3 Position;
    public Vector2 UV;
    public Vector3 Normal;
}