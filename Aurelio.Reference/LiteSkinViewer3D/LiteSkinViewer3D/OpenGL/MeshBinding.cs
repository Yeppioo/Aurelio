namespace LiteSkinViewer3D.OpenGL;

/// <summary>
/// 单个网格绑定项
/// </summary>
internal record MeshBinding {
    public int IndexBufferObject;
    public int VertexArrayObject;
    public int VertexBufferObject;
}

/// <summary>
/// Minecraft风格角色模型的VAO绑定集合
/// </summary>
internal record SteveModelBindings {
    public MeshBinding Head { get; init; } = new();
    public MeshBinding Body { get; init; } = new();
    public MeshBinding LeftArm { get; init; } = new();
    public MeshBinding RightArm { get; init; } = new();
    public MeshBinding LeftLeg { get; init; } = new();
    public MeshBinding RightLeg { get; init; } = new();
    public MeshBinding Cape { get; init; } = new();
}