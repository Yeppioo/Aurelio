using LiteSkinViewer3D.Shared.Enums;

namespace LiteSkinViewer3D.Shared.Interfaces;

public interface IModelIdleAnimation {
    /// <summary>
    /// 动画时长（单位：秒）
    /// </summary>
    float Duration { get; set; }

    /// <summary>
    /// 当前已运行时间（单位：秒）
    /// </summary>
    float Elapsed { get; }

    bool IsExited { get; }
    bool IsExiting { get; }
    bool IsFinished { get; }

    void OnStart(SkinAnimationState state);
    void Tick(SkinAnimationState state, int frame, double deltaTime, SkinType type);
    void ExitedTick(SkinAnimationState state, int frame, double deltaTime, SkinType type);
}