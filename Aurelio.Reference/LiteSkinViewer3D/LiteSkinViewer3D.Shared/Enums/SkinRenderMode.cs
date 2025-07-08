namespace LiteSkinViewer3D.Shared.Enums;

/// <summary>
/// 皮肤渲染抗锯齿模式
/// </summary>
public enum SkinRenderMode {
    /// <summary>
    /// 无抗锯齿（标准渲染）
    /// </summary>
    None,

    /// <summary>
    /// 多重采样抗锯齿（Multisample Anti-Aliasing）
    /// </summary>
    MSAA,

    /// <summary>
    /// 快速近似抗锯齿（Fast Approximate Anti-Aliasing）
    /// </summary>
    FXAA
}
