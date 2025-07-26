using System.Numerics;

namespace LiteSkinViewer3D.Shared.Animations;

/// <summary>
/// 角色动画的当前状态，支持左右身体部件的独立控制
/// </summary>
public sealed class SkinAnimationState {
    public Vector3 Head = Vector3.Zero;
    public Vector3 Body = Vector3.Zero;
    public Vector3 ArmLeft = Vector3.Zero;
    public Vector3 ArmRight = Vector3.Zero;
    public Vector3 LegLeft = Vector3.Zero;
    public Vector3 LegRight = Vector3.Zero;

    public Vector3 HeadTranslation = Vector3.Zero;
    public Vector3 BodyTranslation = Vector3.Zero;
    public Vector3 ArmLeftTranslation = Vector3.Zero;
    public Vector3 ArmRightTranslation = Vector3.Zero;

    public float Cape = 0f;
    public float Time = 0f;
}
