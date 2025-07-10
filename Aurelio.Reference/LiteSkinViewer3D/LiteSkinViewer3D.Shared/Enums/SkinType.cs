namespace LiteSkinViewer3D.Shared.Enums;

/// <summary>
/// 表示皮肤模型的类型（基于 Minecraft 版本）。
/// </summary>
public enum SkinType {
    /// <summary>
    /// 旧版皮肤（1.7及之前，Steve 模型，无袖宽臂）
    /// </summary>
    Legacy,

    /// <summary>
    /// 新版皮肤（1.8及以后，普通身材，Steve 模型）
    /// </summary>
    Classic,

    /// <summary>
    /// 纤细皮肤（1.8及以后，Alex 模型，窄臂）
    /// </summary>
    Slim,

    /// <summary>
    /// 无法识别或未指定的皮肤类型
    /// </summary>
    Unknown
}
