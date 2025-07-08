using LiteSkinViewer3D.Shared.Enums;

namespace LiteSkinViewer3D.Shared.Interfaces;

/// <summary>
/// 模型动画统一接口
/// </summary>
public interface IModelAnimation {

    /// <summary>
    /// 是否启用待机系统
    /// </summary>
    bool EnableIdle { get; }

    /// <summary>
    /// 待机动画
    /// </summary>
    IReadOnlyList<IModelIdleAnimation> IdleAnimations { get; }

    /// <summary>
    /// 在动画进入 Idle 状态时触发
    /// </summary>
    void OnIdleStart(SkinAnimationState state);

    void Tick(SkinAnimationState state, int frame, double deltaTime, SkinType skinType);
}