﻿using System.Numerics;
using LiteSkinViewer3D.Shared.Enums;
using LiteSkinViewer3D.Shared.Helpers;
using LiteSkinViewer3D.Shared.Models;
using SkiaSharp;

namespace LiteSkinViewer3D.Shared;

public abstract class SkinViewerBase
{
    protected readonly SkinAnimationController _skinAnimationController;
    protected bool _animation;

    protected Vector4 _backColor;

    /// <summary>
    ///     披风贴图
    /// </summary>
    protected SKBitmap? _cape;

    protected Vector2 _diffXY;
    protected float _dis = 1;
    protected bool _enableCape;
    protected bool _enableTop;

    protected int _fps;

    protected Matrix4x4 _last;
    protected Vector2 _lastXY;
    protected SkinRenderMode _renderMode;

    protected Vector2 _rotXY;
    protected Vector2 _saveXY;

    /// <summary>
    ///     皮肤贴图
    /// </summary>
    protected SKBitmap? _skinTex;

    protected SkinType _skinType = SkinType.Unknown;
    protected bool _switchBack;
    protected bool _switchModel;
    protected bool _switchSkin;
    protected bool _switchType;
    protected double _time;

    protected Vector2 _xy;

    public SkinViewerBase()
    {
        _last = Matrix4x4.Identity;
        _skinAnimationController = new SkinAnimationController();
    }

    /// <summary>
    ///     是否存在披风
    /// </summary>
    public bool HaveCape { get; protected set; }

    /// <summary>
    ///     是否存在皮肤
    /// </summary>
    public bool HaveSkin { get; protected set; }

    /// <summary>
    ///     画布宽度
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    ///     画布高度
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    ///     渲染器信息
    /// </summary>
    public string Info { get; protected set; }

    /// <summary>
    ///     模型动画
    /// </summary>
    public bool Animation
    {
        get => _animation;
        set => _animation = _skinAnimationController.IsEnable = value;
    }

    /// <summary>
    ///     皮肤类型
    /// </summary>
    public SkinType SkinType
    {
        get => _skinType;
        set
        {
            if (_skinType == value) return;
            _skinType = value;
            _skinAnimationController.SkinType = value;
            _switchModel = true;
        }
    }

    /// <summary>
    ///     背景色
    /// </summary>
    public Vector4 BackColor
    {
        get => _backColor;
        set
        {
            _backColor = value;
            _switchBack = true;
        }
    }

    /// <summary>
    ///     渲染类型
    /// </summary>
    public SkinRenderMode RenderMode
    {
        get => _renderMode;
        set
        {
            _renderMode = value;
            _switchType = true;
        }
    }

    /// <summary>
    ///     是否启用披风渲染
    /// </summary>
    public bool EnableCape
    {
        get => _enableCape;
        set
        {
            _enableCape = value;
            _switchType = true;
        }
    }

    /// <summary>
    ///     是否启用第二层渲染
    /// </summary>
    public bool EnableTop
    {
        get => _enableTop;
        set
        {
            _enableTop = value;
            _switchType = true;
        }
    }

    /// <summary>
    ///     手臂旋转
    /// </summary>
    public Vector3 ArmRotate { get; set; }

    /// <summary>
    ///     腿旋转
    /// </summary>
    public Vector3 LegRotate { get; set; }

    /// <summary>
    ///     头旋转
    /// </summary>
    public Vector3 HeadRotate { get; set; }

    /// <summary>
    ///     渲染出错
    /// </summary>
    public event Action<object?, ErrorType>? Error;

    /// <summary>
    ///     FPS刷新
    /// </summary>
    public event Action<object?, int>? FpsUpdate;

    /// <summary>
    ///     鼠标按下
    /// </summary>
    /// <param name="type"></param>
    /// <param name="point"></param>
    public void PointerPressed(PointerType type, Vector2 point)
    {
        if (type == PointerType.PointerLeft)
        {
            _diffXY.X = point.X;
            _diffXY.Y = -point.Y;
        }
        else if (type == PointerType.PointerRight)
        {
            _lastXY.X = point.X;
            _lastXY.Y = point.Y;
        }
    }

    /// <summary>
    ///     鼠标松开
    /// </summary>
    /// <param name="type"></param>
    /// <param name="point"></param>
    public void PointerReleased(PointerType type, Vector2 point)
    {
        if (type == PointerType.PointerRight)
        {
            _saveXY.X = _xy.X;
            _saveXY.Y = _xy.Y;
        }
    }

    /// <summary>
    ///     鼠标移动
    /// </summary>
    /// <param name="type"></param>
    /// <param name="point"></param>
    public void PointerMoved(PointerType type, Vector2 point)
    {
        if (type == PointerType.PointerLeft)
        {
            _rotXY.Y = point.X - _diffXY.X;
            _rotXY.X = point.Y + _diffXY.Y;
            _rotXY.Y *= 8;
            _rotXY.X *= 8;
            _diffXY.X = point.X;
            _diffXY.Y = -point.Y;
        }
        else if (type == PointerType.PointerRight)
        {
            _xy.X = -(_lastXY.X - point.X) / 100 + _saveXY.X;
            _xy.Y = (_lastXY.Y - point.Y) / 100 + _saveXY.Y;
        }
    }

    /// <summary>
    ///     滚轮
    /// </summary>
    /// <param name="ispost"></param>
    public void PointerWheelChanged(bool ispost)
    {
        if (ispost)
            _dis += 0.1f;
        else
            _dis -= 0.1f;
    }

    /// <summary>
    ///     旋转
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void Rot(float x, float y)
    {
        _rotXY.X += x;
        _rotXY.Y += y;
    }

    /// <summary>
    ///     移动
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void Pos(float x, float y)
    {
        _xy.X += x;
        _xy.Y += y;
    }

    /// <summary>
    ///     缩放
    /// </summary>
    /// <param name="x"></param>
    public void AddDis(float x)
    {
        _dis += x;
    }

    /// <summary>
    ///     设置皮肤贴图
    /// </summary>
    /// <param name="skin"></param>
    /// <exception cref="Exception"></exception>
    public void SetSkinTex(SKBitmap? skin)
    {
        _skinTex?.Dispose();
        if (skin == null)
        {
            HaveSkin = false;
            return;
        }

        _skinTex = skin;

        _skinType = SkinHelper.DetectSkin(skin);
        _switchSkin = true;
        HaveSkin = true;
    }

    /// <summary>
    ///     设置披风贴图
    /// </summary>
    /// <param name="cape"></param>
    public void SetCapeTex(SKBitmap? cape)
    {
        _cape = cape;
        if (cape == null)
        {
            HaveCape = false;
            return;
        }

        _switchSkin = true;
        HaveCape = true;
    }

    /// <summary>
    ///     重置模型
    /// </summary>
    public void ResetPos()
    {
        _dis = 1;
        _diffXY.X = 0;
        _diffXY.Y = 0;
        _xy.X = 0;
        _xy.Y = 0;
        _saveXY.X = 0;
        _saveXY.Y = 0;
        _lastXY.X = 0;
        _lastXY.Y = 0;
        _last = Matrix4x4.Identity;
    }

    /// <summary>
    ///     模型逻辑
    /// </summary>
    /// <param name="time"></param>
    public void Tick(double time)
    {
        if (_animation)
            _skinAnimationController.Tick(time);

        if (_rotXY.X != 0 || _rotXY.Y != 0)
        {
            _last *= Matrix4x4.CreateRotationX(_rotXY.X / 360)
                     * Matrix4x4.CreateRotationY(_rotXY.Y / 360);

            _rotXY.X = 0;
            _rotXY.Y = 0;
        }

        _fps++;
        _time += time;
        if (_time > 1)
        {
            _time -= 1;
            FpsUpdate?.Invoke(this, _fps);
            _fps = 0;
        }
    }

    protected void OnErrorChange(ErrorType data)
    {
        Error?.Invoke(this, data);
    }

    protected Matrix4x4 GetMatrix4(ModelComponent component)
    {
        var enable = _animation;
        var armWidth = _skinType == SkinType.Slim ? 1.375f : 1.5f;

        return component switch
        {
            ModelComponent.Head =>
                Matrix4x4.CreateTranslation(0, Cube.Value * 2.5f, 0) *
                Matrix4x4.CreateRotationZ(GetRot(_skinAnimationController.State.Head, HeadRotate).Z / 360f) *
                Matrix4x4.CreateRotationX(GetRot(_skinAnimationController.State.Head, HeadRotate).X / 360f) *
                Matrix4x4.CreateRotationY(GetRot(_skinAnimationController.State.Head, HeadRotate).Y / 360f) *
                Matrix4x4.CreateTranslation(_skinAnimationController.State.HeadTranslation),

            ModelComponent.Body =>
                Matrix4x4.CreateTranslation(_skinAnimationController.State.BodyTranslation) *
                Matrix4x4.CreateRotationZ(GetRot(_skinAnimationController.State.Body, Vector3.Zero).Z / 360f) *
                Matrix4x4.CreateRotationX(GetRot(_skinAnimationController.State.Body, Vector3.Zero).X / 360f) *
                Matrix4x4.CreateRotationY(GetRot(_skinAnimationController.State.Body, Vector3.Zero).Y / 360f),

            ModelComponent.ArmLeft =>
                Matrix4x4.CreateTranslation(+Cube.Value / 2f, -armWidth * Cube.Value, 0) *
                Matrix4x4.CreateRotationZ(GetRot(_skinAnimationController.State.ArmLeft, ArmRotate).Z / 360f) *
                Matrix4x4.CreateRotationY(GetRot(_skinAnimationController.State.ArmLeft, ArmRotate).Y / 360f) *
                Matrix4x4.CreateRotationX(GetRot(_skinAnimationController.State.ArmLeft, ArmRotate).X / 360f) *
                Matrix4x4.CreateTranslation(armWidth * Cube.Value - Cube.Value / 2f, armWidth * Cube.Value, 0) *
                Matrix4x4.CreateTranslation(_skinAnimationController.State.ArmLeftTranslation),

            ModelComponent.ArmRight =>
                Matrix4x4.CreateTranslation(-Cube.Value / 2f, -armWidth * Cube.Value, 0) *
                Matrix4x4.CreateRotationZ(GetRot(_skinAnimationController.State.ArmRight, ArmRotate, true).Z / 360f) *
                Matrix4x4.CreateRotationY(GetRot(_skinAnimationController.State.ArmRight, ArmRotate, true).Y / 360f) *
                Matrix4x4.CreateRotationX(GetRot(_skinAnimationController.State.ArmRight, ArmRotate, true).X / 360f) *
                Matrix4x4.CreateTranslation(-armWidth * Cube.Value + Cube.Value / 2f, armWidth * Cube.Value, 0) *
                Matrix4x4.CreateTranslation(_skinAnimationController.State.ArmRightTranslation),

            ModelComponent.LegLeft =>
                Matrix4x4.CreateTranslation(0, -1.5f * Cube.Value, 0) *
                Matrix4x4.CreateRotationZ(GetRot(_skinAnimationController.State.LegLeft, LegRotate).Z / 360f) *
                Matrix4x4.CreateRotationY(GetRot(_skinAnimationController.State.LegLeft, LegRotate).Y / 360f) *
                Matrix4x4.CreateRotationX(GetRot(_skinAnimationController.State.LegLeft, LegRotate).X / 360f) *
                Matrix4x4.CreateTranslation(Cube.Value * 0.5f, -Cube.Value * 1.5f, 0),

            ModelComponent.LegRight =>
                Matrix4x4.CreateTranslation(0, -1.5f * Cube.Value, 0) *
                Matrix4x4.CreateRotationZ(GetRot(_skinAnimationController.State.LegRight, LegRotate, true).Z / 360f) *
                Matrix4x4.CreateRotationY(GetRot(_skinAnimationController.State.LegRight, LegRotate, true).Y / 360f) *
                Matrix4x4.CreateRotationX(GetRot(_skinAnimationController.State.LegRight, LegRotate, true).X / 360f) *
                Matrix4x4.CreateTranslation(-Cube.Value * 0.5f, -Cube.Value * 1.5f, 0),

            ModelComponent.Cape =>
                Matrix4x4.CreateTranslation(0, -2f * Cube.Value, -Cube.Value * 0.1f) *
                Matrix4x4.CreateRotationX((float)((enable ? 11.8 + _skinAnimationController.State.Cape : 6.3) *
                    Math.PI / 180)) *
                Matrix4x4.CreateTranslation(0, 1.6f * Cube.Value, -Cube.Value * 0.5f),

            ModelComponent.ModelMatrix =>
                _last *
                Matrix4x4.CreateTranslation(new Vector3(_xy.X, _xy.Y, 0)) *
                Matrix4x4.CreateScale(_dis),

            ModelComponent.ProjectionMatrix =>
                Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4, (float)Width / Height, 0.1f, 10.0f),

            ModelComponent.ViewMatrix =>
                Matrix4x4.CreateLookAt(new Vector3(0, 0, 7), Vector3.Zero, Vector3.UnitY),

            _ => Matrix4x4.Identity
        };

        Vector3 GetRot(Vector3 animated, Vector3 manual, bool mirror = false)
        {
            var v = enable ? animated : manual;
            return mirror ? new Vector3(-v.X, -v.Y, -v.Z) : v;
        }
    }
}