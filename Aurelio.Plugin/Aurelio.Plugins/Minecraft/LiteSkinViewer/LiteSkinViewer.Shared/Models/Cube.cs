using System.Numerics;

namespace LiteSkinViewer3D.Shared.Models;

/// <summary>
/// 用于构造标准立方体的顶点坐标、法向量和索引
/// </summary>
public static class Cube {
    /// <summary>
    /// 方块半边长度，整个边长为 1 单位
    /// </summary>
    public const float Value = 0.5f;

    public static readonly float[] Vertices =
[
    0.0f,  0.0f, -1.0f,
        0.0f,  0.0f, -1.0f,
        0.0f,  0.0f, -1.0f,
        0.0f,  0.0f, -1.0f,

        0.0f,  0.0f,  1.0f,
        0.0f,  0.0f,  1.0f,
        0.0f,  0.0f,  1.0f,
        0.0f,  0.0f,  1.0f,

        -1.0f,  0.0f,  0.0f,
        -1.0f,  0.0f,  0.0f,
        -1.0f,  0.0f,  0.0f,
        -1.0f,  0.0f,  0.0f,

        1.0f,  0.0f,  0.0f,
        1.0f,  0.0f,  0.0f,
        1.0f,  0.0f,  0.0f,
        1.0f,  0.0f,  0.0f,

        0.0f,  1.0f,  0.0f,
        0.0f,  1.0f,  0.0f,
        0.0f,  1.0f,  0.0f,
        0.0f,  1.0f,  0.0f,

        0.0f, -1.0f,  0.0f,
        0.0f, -1.0f,  0.0f,
        0.0f, -1.0f,  0.0f,
        0.0f, -1.0f,  0.0f,
    ];

    /// <summary>
    /// 方块的 24 个顶点位置坐标（按面展开）
    /// 每个面 4 个点，共 6 个面
    /// </summary>
    private static readonly Vector3[] _positions = [
        // Back (-Z)
        new(Value, Value, -Value),
        new(Value, -Value, -Value),
        new(-Value, -Value, -Value),
        new(-Value, Value, -Value),

        // Front (+Z)
        new(-Value, Value, Value),
        new(-Value, -Value, Value),
        new(Value, -Value, Value),
        new(Value, Value, Value),

        // Left (-X)
        new(-Value, Value, -Value),
        new(-Value, -Value, -Value),
        new(-Value, -Value, Value),
        new(-Value, Value, Value),

        // Right (+X)
        new(Value, Value, Value),
        new(Value, -Value, Value),
        new(Value, -Value, -Value),
        new(Value, Value, -Value),

        // Top (+Y)
        new(-Value, Value, -Value),
        new(-Value, Value, Value),
        new(Value, Value, Value),
        new(Value, Value, -Value),

        // Bottom (-Y)
        new(Value, -Value, -Value),
        new(Value, -Value, Value),
        new(-Value, -Value, Value),
        new(-Value, -Value, -Value),
    ];

    /// <summary>
    /// 每个顶点对应的法线（方向指向该面外侧），用于光照计算
    /// 顺序与 _positions 匹配
    /// </summary>
    private static readonly Vector3[] _normals = [
        // Back
        Vector3.UnitZ * -1, Vector3.UnitZ * -1, Vector3.UnitZ * -1, Vector3.UnitZ * -1,
        // Front
        Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ,
        // Left
        Vector3.UnitX * -1, Vector3.UnitX * -1, Vector3.UnitX * -1, Vector3.UnitX * -1,
        // Right
        Vector3.UnitX, Vector3.UnitX, Vector3.UnitX, Vector3.UnitX,
        // Top
        Vector3.UnitY, Vector3.UnitY, Vector3.UnitY, Vector3.UnitY,
        // Bottom
        Vector3.UnitY * -1, Vector3.UnitY * -1, Vector3.UnitY * -1, Vector3.UnitY * -1,
    ];

    /// <summary>
    /// 绘制立方体的顶点索引（按三角面排列）
    /// 每个面两个三角形 → 共 36 个索引
    /// </summary>
    private static readonly ushort[] _indices =
    {
        0, 1, 2, 0, 2, 3,       // Back
        4, 5, 6, 4, 6, 7,       // Front
        8, 9, 10, 8, 10, 11,    // Left
        12, 13, 14, 12, 14, 15, // Right
        16, 17, 18, 16, 18, 19, // Top
        20, 21, 22, 20, 22, 23  // Bottom
    };

    /// <summary>
    /// 获取一个经缩放/偏移后的立方体顶点列表
    /// </summary>
    /// <param name="scale">缩放比例（可为向量）</param>
    /// <param name="offset">偏移向量</param>
    /// <returns>处理后的顶点坐标数组</returns>
    public static Vector3[] GetTransformedVertices(Vector3 scale, Vector3 offset) {
        var result = new Vector3[_positions.Length];
        for (int i = 0; i < _positions.Length; i++) {
            result[i] = Vector3.Multiply(_positions[i], scale) + offset;
        }
        return result;
    }

    /// <summary>
    /// 获取该立方体的法线数组（方向不会因缩放偏移变化）
    /// </summary>
    public static Vector3[] GetNormals() => _normals;

    /// <summary>
    /// 获取索引数组，并为每个索引添加偏移
    /// </summary>
    /// <param name="offset">每个索引添加的起始偏移量</param>
    public static ushort[] GetIndices(int offset = 0) {
        var result = new ushort[_indices.Length];
        for (int i = 0; i < _indices.Length; i++) {
            result[i] = (ushort)(_indices[i] + offset);
        }
        return result;
    }
}