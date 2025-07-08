using LiteSkinViewer3D.Shared.Enums;
using LiteSkinViewer3D.Shared.Interfaces;
using System.Numerics;

namespace LiteSkinViewer3D.Shared;

/// <summary>
/// 角色动画的当前状态，支持左右身体部件的独立控制
/// </summary>
public sealed class SkinAnimationState {
    public Vector3 Body = Vector3.Zero;
    public Vector3 ArmLeft = Vector3.Zero;
    public Vector3 ArmRight = Vector3.Zero;
    public Vector3 LegLeft = Vector3.Zero;
    public Vector3 LegRight = Vector3.Zero;
    public Vector3 Head = Vector3.Zero;

    public float Cape = 0f;
    public float Time = 0f;
}

/// <summary>
/// 皮肤动画控制器
/// </summary>
public class SkinAnimationController {
    private int _frame = 0;
    private double _tickAccum = 0f;
    private double _idleTimer = 0f;
    private bool _closed = false;
    private readonly Random _rng = new();

    private IModelIdleAnimation? _currentIdle;

    public IModelAnimation Controller { get; set; }
    public SkinAnimationState State { get; } = new();
    public SkinType SkinType { get; set; }
    public bool IsEnable { get; set; } = true;
    public float IdleIntervalSeconds { get; set; } = 5f;

    public SkinAnimationController(IModelAnimation? controller = null) {
        Controller = controller ?? new DefaultAnimation();
    }

    public void Close() {
        _closed = true;
        IsEnable = false;
    }

    public bool Tick(double deltaTime) {
        if (!IsEnable) return !_closed;

        _tickAccum += deltaTime;
        _idleTimer += deltaTime;

        while (_tickAccum >= 0.01f) {
            _tickAccum -= 0.01f;
            _frame = (_frame + 1) % 120;
        }

        if (_currentIdle != null) {
            if (!_currentIdle.IsFinished) {
                _currentIdle.Tick(State, _frame, deltaTime, SkinType);
            } else if (!_currentIdle.IsExited) {
                _currentIdle.ExitedTick(State, _frame, deltaTime, SkinType);
            } else {
                _currentIdle = null;
                _idleTimer = 0f;
            }
        } else {
            if (_idleTimer >= IdleIntervalSeconds &&
                Controller.EnableIdle &&
                Controller.IdleAnimations.Count > 0) {
                _idleTimer = 0f;
                var pool = Controller.IdleAnimations;
                _currentIdle = pool.Count == 1 ? pool[0] : pool[_rng.Next(pool.Count)];
                _currentIdle.OnStart(State);
            }

            Controller.Tick(State, _frame, deltaTime, SkinType);
        }

        return !_closed;
    }
}

public sealed class DefaultAnimation : IModelAnimation {
    public bool EnableIdle => true;

    public IReadOnlyList<IModelIdleAnimation> IdleAnimations => [
        new CuddleAnimation(),
        new LookAroundAnimation(),
    ];

    public void OnIdleStart(SkinAnimationState state) {
        state.Time = 0f;
    }

    public void Tick(SkinAnimationState state, int frame, double deltaTime, SkinType type) {
        state.Time += (float)deltaTime * 1f; // 节奏调慢

        float t = state.Time * MathF.PI;
        float armZ = MathF.Sin(t * 0.9f) * 7.0f;
        float armY = MathF.Sin(t * 0.4f) * 13.0f;

        state.ArmLeft.Z = armZ;
        state.ArmRight.Z = armZ;
        state.ArmLeft.Y = armY;
        state.ArmRight.Y = armY;
        state.Head.Y = MathF.Sin(t * 0.8f + 0.5f) * 5.0f;
        state.Cape = 0.25f + MathF.Sin(t * 0.85f + 0.5f) * 0.5f;
    }
}

public sealed class LookAroundAnimation : IModelIdleAnimation {
    public float Elapsed => _time;
    public bool IsFinished => _time >= Duration;
    public bool IsExiting => _exiting;
    public bool IsExited => _exitTime >= 0.4f && IsNearlyZero(_currentZ); public float Duration { get; set; } = 10.0f;

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

        if (_switchTimer >= _switchInterval) {
            _switchTimer = 0f;
            _targetYaw = GetRandomYaw();
            _switchInterval = 1.2f + (float)_rng.NextDouble() * 1.0f;
        }

        if (_lookShockTimer >= 7.5f) {
            _lookShockTimer = 0f;
            _targetYaw = _rng.Next(0, 2) == 0 ? -100f : 100f;
        }

        _yaw = SmoothApproach(_yaw, _targetYaw, (float)deltaTime, 0.25f);
        float jitter = MathF.Sin(_time * 13.3f + 3.1f) * 0.5f;

        float t = _time * MathF.PI;
        float armZ = MathF.Sin(t * 0.9f) * 7.0f;
        float armY = MathF.Sin(t * 0.4f) * 13.0f;

        state.Head.Z = _yaw + jitter;
        state.Head.Y = MathF.Sin(_time * 1.7f + 0.2f) * 5.5f;

        state.ArmLeft.Z = armZ;
        state.ArmRight.Z = armZ;
        state.ArmLeft.Y = armY;
        state.ArmRight.Y = armY;

        state.Body.Z = MathF.Sin(_time * 1.6f) * 3.2f - 2.5f;
        state.Cape = 0.25f + MathF.Sin(_time * 1.2f + 0.4f) * 0.25f;

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

public sealed class CuddleAnimation : IModelIdleAnimation {
    public float Duration { get; set; } = 4.0f;
    public float PlaybackDuration { get; set; } = 1.0f;
    public float ExitSmoothTime { get; set; } = 1.0f;

    public float Elapsed => _lifeTime;
    public bool IsFinished => _lifeTime >= Duration;
    public bool IsExiting => _exiting;
    public bool IsExited => _exitTime >= 0.4f && AllZero();

    private float _lifeTime = 0f;
    private float _playTime = 0f;
    private float _exitTime = 0f;
    private bool _exiting = false;
    private bool _poseLocked = false;

    private float _rightX, _rightY, _rightZ;
    private float _leftX, _leftY, _leftZ;
    private float _headX, _headY, _headZ;
    private float _bodyX, _bodyY, _bodyZ;
    private float _legLY, _legRY;
    private float _cape;

    private float _t = 0f;

    public void OnStart(SkinAnimationState state) {
        _lifeTime = 0f;
        _playTime = 0f;
        _exitTime = 0f;
        _exiting = false;
        _poseLocked = false;
        _t = -PlaybackDuration;
    }

    public void Tick(SkinAnimationState state, int frame, double deltaTime, SkinType type) {
        float dt = (float)deltaTime;
        _lifeTime += dt;
        _t += dt;

        if (!_poseLocked) {
            _playTime += dt;
            float tNorm = MathF.Min(_playTime / PlaybackDuration, 1f);
            float ease = EaseOut(tNorm);

            _leftX = Lerp(0, 280, ease);
            _leftY = Lerp(0, -460, ease);
            _leftZ = Lerp(0, 280, ease);

            _rightX = Lerp(0, 280, ease);
            _rightY = Lerp(0, 460, ease);
            _rightZ = Lerp(0, 280, ease);

            _headX = Lerp(0, 40, ease);
            _headY = Lerp(0, -40, ease);
            _headZ = 0f;

            _bodyX = 0f;
            _bodyY = Lerp(0, -30, ease);
            _bodyZ = 0f;

            _legLY = Lerp(0, 30, ease);
            _legRY = Lerp(0, -30, ease);

            _cape = Lerp(0, -5, ease);

            if (_playTime >= PlaybackDuration)
                _poseLocked = true;
        } else {
            float wiggle = MathF.Sin(_t * 1.8f) * 6f;
            float pulse = MathF.Sin(_t * 2.4f + 1f) * 9f;
            float nod = MathF.Sin(_t * 1.2f + 0.8f) * 5f;

            _rightY = 460f + pulse;
            _rightZ = 280f + wiggle;

            _leftY = -460f + pulse * 0.7f;
            _leftZ = 280f - wiggle * 0.85f;

            _headY = -40f + MathF.Sin(_t * 1.4f) * 4.5f;
            _headZ = MathF.Sin(_t * 1.1f + 0.3f) * 3.2f;
            _headX = 40f + nod;

            _cape = -5f + MathF.Sin(_t * 1.9f + 1.5f) * 0.5f;
        }

        ApplyState(state);
        if (!_exiting && IsFinished)
            _exiting = true;
    }

    public void ExitedTick(SkinAnimationState state, int frame, double deltaTime, SkinType type) {
        float dt = (float)deltaTime;
        _exitTime += dt;

        _leftX = SmoothApproach(_leftX, 0f, dt, ExitSmoothTime);
        _leftY = SmoothApproach(_leftY, 0f, dt, ExitSmoothTime);
        _leftZ = SmoothApproach(_leftZ, 0f, dt, ExitSmoothTime);

        _rightX = SmoothApproach(_rightX, 0f, dt, ExitSmoothTime);
        _rightY = SmoothApproach(_rightY, 0f, dt, ExitSmoothTime);
        _rightZ = SmoothApproach(_rightZ, 0f, dt, ExitSmoothTime);

        _headX = SmoothApproach(_headX, 0f, dt, ExitSmoothTime);
        _headY = SmoothApproach(_headY, 0f, dt, ExitSmoothTime);
        _headZ = SmoothApproach(_headZ, 0f, dt, ExitSmoothTime);

        _bodyX = SmoothApproach(_bodyX, 0f, dt, ExitSmoothTime);
        _bodyY = SmoothApproach(_bodyY, 0f, dt, ExitSmoothTime);
        _bodyZ = SmoothApproach(_bodyZ, 0f, dt, ExitSmoothTime);

        _legLY = SmoothApproach(_legLY, 0f, dt, ExitSmoothTime);
        _legRY = SmoothApproach(_legRY, 0f, dt, ExitSmoothTime);

        _cape = SmoothApproach(_cape, 0f, dt, ExitSmoothTime);

        ApplyState(state);
    }

    private void ApplyState(SkinAnimationState state) {
        state.ArmLeft.X = _leftX;
        state.ArmLeft.Y = _leftY;
        state.ArmLeft.Z = _leftZ;

        state.ArmRight.X = _rightX;
        state.ArmRight.Y = _rightY;
        state.ArmRight.Z = _rightZ;

        state.Head.X = _headX;
        state.Head.Y = _headY;
        state.Head.Z = _headZ;

        state.Body.X = _bodyX;
        state.Body.Y = _bodyY;
        state.Body.Z = _bodyZ;

        state.LegLeft.Y = _legLY;
        state.LegRight.Y = _legRY;

        state.Cape = _cape;
    }

    private static float Lerp(float a, float b, float t) =>
        a + (b - a) * t;

    private static float EaseOut(float x) =>
        1f - MathF.Pow(1f - x, 3f);

    private static float SmoothApproach(float current, float target, float dt, float smoothTime) =>
        current + (target - current) * (1f - MathF.Exp(-dt * 10f / smoothTime));

    private static bool IsNearlyZero(float v) => MathF.Abs(v) < 0.05f;

    private bool AllZero() =>
        IsNearlyZero(_rightX) && IsNearlyZero(_rightY) && IsNearlyZero(_rightZ) &&
        IsNearlyZero(_leftX) && IsNearlyZero(_leftY) && IsNearlyZero(_leftZ) &&
        IsNearlyZero(_headX) && IsNearlyZero(_headY) && IsNearlyZero(_headZ) &&
        IsNearlyZero(_bodyX) && IsNearlyZero(_bodyY) && IsNearlyZero(_bodyZ) &&
        IsNearlyZero(_legLY) && IsNearlyZero(_legRY) && IsNearlyZero(_cape);
}
