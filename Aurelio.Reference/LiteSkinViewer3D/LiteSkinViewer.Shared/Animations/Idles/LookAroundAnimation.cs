using LiteSkinViewer3D.Shared.Enums;
using LiteSkinViewer3D.Shared.Interfaces;

namespace LiteSkinViewer3D.Shared.Animations.Idles;

public sealed class LookAroundAnimation : IModelIdleAnimation
{
    private readonly Random _rng = new();
    private float _currentZ;
    private float _exitTime;
    private float _lookShockTimer;
    private float _switchInterval = 1.5f;
    private float _switchTimer;
    private float _targetYaw;

    private float _yaw;
    public float Elapsed { get; private set; }

    public bool IsFinished => Elapsed >= Duration;
    public bool IsExiting { get; private set; }

    public bool IsExited => _exitTime >= 0.4f && IsNearlyZero(_currentZ);
    public float Duration { get; set; } = 10.0f;

    public void OnStart(SkinAnimationState state)
    {
        Elapsed = 0f;
        _yaw = state.Head.Z;
        _targetYaw = GetRandomYaw();
        _switchTimer = 0f;
    }

    public void Tick(SkinAnimationState state, int frame, double deltaTime, SkinType type)
    {
        Elapsed += (float)deltaTime;
        _switchTimer += (float)deltaTime;
        _lookShockTimer += (float)deltaTime;

        // 随机切换注视方向
        if (_switchTimer >= _switchInterval)
        {
            _switchTimer = 0f;
            _targetYaw = GetRandomYaw();
            _switchInterval = 1.2f + (float)_rng.NextDouble() * 1.0f;
        }

        // 强制触发快速转头
        if (_lookShockTimer >= 7.5f)
        {
            _lookShockTimer = 0f;
            _targetYaw = _rng.Next(0, 2) == 0 ? -100f : 100f;
        }

        _yaw = SmoothApproach(_yaw, _targetYaw, (float)deltaTime, 0.25f);
        var jitter = MathF.Sin(Elapsed * 13.3f + 3.1f) * 0.5f;

        var t = Elapsed * MathF.PI;
        var sinT = MathF.Sin(t);
        var sinHalfT = MathF.Sin(t * 0.5f);
        var armSwing = (sinHalfT + 1f) * 20f;

        // Head movement
        state.Head.Y = _yaw + jitter;
        state.Head.X = MathF.Sin(Elapsed * 1.7f) * 5f;

        // Arm movement
        state.ArmLeft.Z = armSwing;
        state.ArmRight.Z = armSwing;

        state.ArmLeft.Y = sinT * 10f;
        state.ArmRight.Y = -sinT * 10f;

        // Body twist
        state.Body.Z = MathF.Sin(Elapsed * 1.6f) * 3.2f - 2.5f;

        // Head/body floating
        var floatY = MathF.Sin(Elapsed * 2.2f) * 0.01f;
        state.HeadTranslation.Y = floatY;
        state.BodyTranslation.Y = floatY;
        state.ArmLeftTranslation.Y = floatY;
        state.ArmRightTranslation.Y = floatY;

        // Cape sway
        state.Cape = 0.5f + MathF.Sin(Elapsed * 1.4f) * 0.35f;
        if (IsFinished && !IsExiting) IsExiting = true;
    }

    public void ExitedTick(SkinAnimationState state, int frame, double deltaTime, SkinType type)
    {
        _exitTime += (float)deltaTime;

        _currentZ = SmoothApproach(_currentZ, 0f, (float)deltaTime, 0.25f);
        var headX = SmoothApproach(state.Head.X, 0f, (float)deltaTime, 0.25f);

        state.Head.Z = _currentZ;
        state.Head.X = headX;
    }

    private float GetRandomYaw()
    {
        return _rng.Next(3) switch
        {
            0 => -50f,
            1 => 0f,
            _ => 50f
        };
    }

    private static float SmoothApproach(float current, float target, float dt, float smoothTime)
    {
        var t = 1f - MathF.Exp(-dt * 10f / smoothTime);
        return current + (target - current) * t;
    }

    private static bool IsNearlyZero(float v)
    {
        return MathF.Abs(v) < 0.05f;
    }
}