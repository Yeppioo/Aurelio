using System.Numerics;

namespace LiteSkinViewer3D.Shared.Extensions;
public static class VectorExtension {
    /// <summary>
    /// 将 Vector3 数组展开为 float 数组
    /// </summary>
    public static float[] ToFloatArray(this Vector3[] vectors) {
        var result = new float[vectors.Length * 3];
        for (int i = 0; i < vectors.Length; i++) {
            result[i * 3 + 0] = vectors[i].X;
            result[i * 3 + 1] = vectors[i].Y;
            result[i * 3 + 2] = vectors[i].Z;
        }

        return result;
    }
}