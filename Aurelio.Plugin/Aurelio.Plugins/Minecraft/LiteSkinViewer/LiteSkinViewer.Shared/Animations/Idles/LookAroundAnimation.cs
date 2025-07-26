using LiteSkinViewer3D.Shared.Enums;
using LiteSkinViewer3D.Shared.Interfaces;

namespace LiteSkinViewer3D.Shared.Animations.Idles;

public sealed class LookAroundAnimation : IModelIdleAnimation {
    public float Elapsed => _time;
    public bool IsFinished => _time >= Duration;
    public bool IsExiting => _exiting;
    public bool IsExited => _exitTime >= 0.4f && IsNearlyZero(_currentZ);
    public float Duration { get; set; } = 10.0f;

    private float _time = 0f;
    private float _yaw = 0f;
    private float _currentZ = 0f;
    private float _exitTime = 0f;
    private float _targetYaw = 0f;
    private float _switchTimer = 0f;
    private float _lookShockTimer = 0f;
    private float _switchInterval = 1.5f;
    private bool _exiting = false;
    private readonly Random _rng = new();

    public void OnStart(SkinAnimationState state) {
        _time = 0f;
        _yaw = state.Head.Z;
        _targetYaw = GetRandomYaw();
        _switchTimer = 0f;
    }

    public void Tick(SkinAnimationState state, int frame, double deltaTime, SkinType type) {
        _time += (float)deltaTime;
        _switchTimer += (float)deltaTime;
        _lookShockTimer += (float)deltaTime;

        // 随机切换注视方向
        if (_switchTimer >= _switchInterval) {
            _switchTimer = 0f;
            _targetYaw = GetRandomYaw();
            _switchInterval = 1.2f + (float)_rng.NextDouble() * 1.0f;
        }

        // 强制触发快速转头
        if (_lookShockTimer >= 7.5f) {
            _lookShockTimer = 0f;
            _targetYaw = _rng.Next(0, 2) == 0 ? -100f : 100f;
        }

        _yaw = SmoothApproach(_yaw, _targetYaw, (float)deltaTime, 0.25f);
        float jitter = MathF.Sin(_time * 13.3f + 3.1f) * 0.5f;

        float t = _time * MathF.PI;
        float sinT = MathF.Sin(t);
        float sinHalfT = MathF.Sin(t * 0.5f);
        float armSwing = (sinHalfT + 1f) * 20f;

        // Head movement
        state.Head.Y = _yaw + jitter;
        state.Head.X = MathF.Sin(_time * 1.7f) * 5f;

        // Arm movement
        state.ArmLeft.Z = armSwing;
        state.ArmRight.Z = armSwing;

        state.ArmLeft.Y = sinT * 10f;
        state.ArmRight.Y = -sinT * 10f;

        // Body twist
        state.Body.Z = MathF.Sin(_time * 1.6f) * 3.2f - 2.5f;

        // Head/body floating
        float floatY = MathF.Sin(_time * 2.2f) * 0.01f;
        state.HeadTranslation.Y = floatY;
        state.BodyTranslation.Y = floatY;
        state.ArmLeftTranslation.Y = floatY;
        state.ArmRightTranslation.Y = floatY;

        // Cape sway
        state.Cape = 0.5f + MathF.Sin(_time * 1.4f) * 0.35f;
        if (IsFinished && !_exiting) _exiting = true;
    }

    public void ExitedTick(SkinAnimationState state, int frame, double deltaTime, SkinType type) {
        _exitTime += (float)deltaTime;

        _currentZ = SmoothApproach(_currentZ, 0f, (float)deltaTime, 0.25f);
        float headX = SmoothApproach(state.Head.X, 0f, (float)deltaTime, 0.25f);

        state.Head.Z = _currentZ;
        state.Head.X = headX;
    }

    private float GetRandomYaw() => _rng.Next(3) switch {
        0 => -50f,
        1 => 0f,
        _ => 50f
    };

    private static float SmoothApproach(float current, float target, float dt, float smoothTime) {
        float t = 1f - MathF.Exp(-dt * 10f / smoothTime);
        return current + (target - current) * t;
    }

    private static bool IsNearlyZero(float v) => MathF.Abs(v) < 0.05f;
}