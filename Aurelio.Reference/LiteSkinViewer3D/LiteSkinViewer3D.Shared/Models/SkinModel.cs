namespace LiteSkinViewer3D.Shared.Models;

/// <summary>
/// 表示模型网格的一个部
/// </summary>
public record CubeItemModel {
    public float[] Vertices;
    public ushort[] Indices;
}

/// <summary>
/// 史蒂夫样式模型
/// </summary>
public record SteveMeshModel {
    public CubeItemModel? Cape;

    public CubeItemModel Head;
    public CubeItemModel Body;
    public CubeItemModel LeftArm;
    public CubeItemModel RightArm;
    public CubeItemModel LeftLeg;
    public CubeItemModel RightLeg;
}

/// <summary>
/// 史蒂夫皮肤贴图的 UV 坐标布局
/// </summary>
public record SteveTextureLayout {
    public float[]? Cape;

    public float[] Head;
    public float[] Body;
    public float[] LeftArm;
    public float[] RightArm;
    public float[] LeftLeg;
    public float[] RightLeg;
}