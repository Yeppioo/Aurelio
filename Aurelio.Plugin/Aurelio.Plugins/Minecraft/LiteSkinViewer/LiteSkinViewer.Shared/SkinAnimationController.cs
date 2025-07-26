using LiteSkinViewer3D.Shared.Animations;
using LiteSkinViewer3D.Shared.Enums;
using LiteSkinViewer3D.Shared.Interfaces;
using System.Numerics;

namespace LiteSkinViewer3D.Shared;

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

    public SkinAnimationState State { get; } = new();
    public IModelAnimation Controller { get; set; }
    public SkinType SkinType { get; set; }
    public bool IsEnable { get; set; } = true;
    public float IdleIntervalSeconds { get; set; } = 15f;

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