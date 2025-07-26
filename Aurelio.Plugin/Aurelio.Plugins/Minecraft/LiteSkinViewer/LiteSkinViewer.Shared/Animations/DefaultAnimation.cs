using LiteSkinViewer3D.Shared.Animations.Idles;
using LiteSkinViewer3D.Shared.Enums;
using LiteSkinViewer3D.Shared.Interfaces;

namespace LiteSkinViewer3D.Shared.Animations;

public sealed class DefaultAnimation : IModelAnimation {
    public bool EnableIdle => true;

    public IReadOnlyList<IModelIdleAnimation> IdleAnimations { get; set; } = [
        new LookAroundAnimation(),
    ];

    public void OnIdleStart(SkinAnimationState state) {
        state.Time = 0f;
    }

    public void Tick(SkinAnimationState state, int frame, double deltaTime, SkinType type) {
        state.Time += (float)deltaTime * 4f;
        float t = state.Time;

        float sinT = MathF.Sin(t);
        float sinHalfT = MathF.Sin(t * 0.5f);
        float armSwing = (sinHalfT + 1f) * 20f;

        state.ArmLeft.Z = armSwing;
        state.ArmRight.Z = armSwing;

        state.ArmLeft.Y = sinT * 10f;
        state.ArmRight.Y = -sinT * 10f;

        state.Head.X = sinHalfT * 5f;
        state.HeadTranslation.Y = sinT * 0.008f;

        state.BodyTranslation.Y = sinT * 0.01f;
        state.ArmRightTranslation.Y = state.BodyTranslation.Y;
        state.ArmLeftTranslation.Y = state.BodyTranslation.Y;

        state.Cape = 0.5f + sinT * 0.5f;
    }
}