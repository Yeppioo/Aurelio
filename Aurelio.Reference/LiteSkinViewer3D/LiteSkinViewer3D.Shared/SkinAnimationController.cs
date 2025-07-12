using LiteSkinViewer3D.Shared.Animations;
using LiteSkinViewer3D.Shared.Enums;
using LiteSkinViewer3D.Shared.Interfaces;

namespace LiteSkinViewer3D.Shared;

/// <summary>
///     皮肤动画控制器
/// </summary>
public class SkinAnimationController
{
    private readonly Random _rng = new();
    private bool _closed;

    private IModelIdleAnimation? _currentIdle;
    private int _frame;
    private double _idleTimer;
    private double _tickAccum;

    public SkinAnimationController(IModelAnimation? controller = null)
    {
        Controller = controller ?? new DefaultAnimation();
    }

    public IModelAnimation Controller { get; set; }
    public SkinAnimationState State { get; } = new();
    public SkinType SkinType { get; set; }
    public bool IsEnable { get; set; } = true;
    public float IdleIntervalSeconds { get; set; } = 1f;

    public void Close()
    {
        _closed = true;
        IsEnable = false;
    }

    public bool Tick(double deltaTime)
    {
        if (!IsEnable) return !_closed;

        _tickAccum += deltaTime;
        _idleTimer += deltaTime;

        while (_tickAccum >= 0.01f)
        {
            _tickAccum -= 0.01f;
            _frame = (_frame + 1) % 120;
        }

        if (_currentIdle != null)
        {
            if (!_currentIdle.IsFinished)
            {
                _currentIdle.Tick(State, _frame, deltaTime, SkinType);
            }
            else if (!_currentIdle.IsExited)
            {
                _currentIdle.ExitedTick(State, _frame, deltaTime, SkinType);
            }
            else
            {
                _currentIdle = null;
                _idleTimer = 0f;
            }
        }
        else
        {
            if (_idleTimer >= IdleIntervalSeconds &&
                Controller.EnableIdle &&
                Controller.IdleAnimations.Count > 0)
            {
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