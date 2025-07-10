using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using LiteSkinViewer3D.OpenGL;
using LiteSkinViewer3D.Shared.Enums;
using LiteSkinViewer3D.Shared.Helpers;
using SkiaSharp;
using System.Diagnostics;
using System.Numerics;

namespace LiteSkinViewer3D.Avalonia.Controls;

public sealed class SkinViewer3D : OpenGlControlBase, ICustomHitTest {
    private static readonly Vector2 DefaultModelPosition = new(0f, 0.5f);

    private DateTime _time;
    private float _modelDistance;
    private OpenGLSkinViewerBase? _skin;

    public static readonly StyledProperty<Bitmap> SkinProperty =
        AvaloniaProperty.Register<SkinViewer3D, Bitmap>(nameof(Skin));

    public static readonly StyledProperty<Bitmap> CapeProperty =
        AvaloniaProperty.Register<SkinViewer3D, Bitmap>(nameof(Cape));

    public static readonly StyledProperty<bool> IsEnableAnimationProperty =
        AvaloniaProperty.Register<SkinViewer3D, bool>(nameof(IsEnableAnimation), true);

    public static readonly StyledProperty<bool> IsCapeVisibleProperty =
        AvaloniaProperty.Register<SkinViewer3D, bool>(nameof(IsCapeVisible), true);

    public static readonly StyledProperty<bool> IsUpperLayerVisibleProperty =
        AvaloniaProperty.Register<SkinViewer3D, bool>(nameof(IsUpperLayerVisible), true);

    public static readonly StyledProperty<SkinRenderMode> RenderModeProperty =
        AvaloniaProperty.Register<SkinViewer3D, SkinRenderMode>(nameof(RenderMode), SkinRenderMode.FXAA);

    public static readonly StyledProperty<Vector3> HeadRotationProperty =
        AvaloniaProperty.Register<SkinViewer3D, Vector3>(nameof(HeadRotation));

    public static readonly StyledProperty<Vector2> ArmRotationProperty =
        AvaloniaProperty.Register<SkinViewer3D, Vector2>(nameof(ArmRotation));

    public static readonly StyledProperty<Vector2> LegRotationProperty =
        AvaloniaProperty.Register<SkinViewer3D, Vector2>(nameof(LegRotation));

    public static readonly StyledProperty<float> ModelDistanceProperty =
        AvaloniaProperty.Register<SkinViewer3D, float>(nameof(ModelDistance), 1f);

    public static readonly StyledProperty<Vector2> ModelRotationProperty =
        AvaloniaProperty.Register<SkinViewer3D, Vector2>(nameof(ModelRotation));

    public static readonly StyledProperty<Vector2> ModelPositionProperty =
        AvaloniaProperty.Register<SkinViewer3D, Vector2>(nameof(ModelPosition), DefaultModelPosition);

    public Bitmap Skin {
        get => GetValue(SkinProperty);
        set => SetValue(SkinProperty, value);
    }

    public Bitmap Cape {
        get => GetValue(CapeProperty);
        set => SetValue(CapeProperty, value);
    }

    public bool IsCapeVisible {
        get => GetValue(IsCapeVisibleProperty);
        set => SetValue(IsCapeVisibleProperty, value);
    }

    public bool IsEnableAnimation {
        get => GetValue(IsEnableAnimationProperty);
        set => SetValue(IsEnableAnimationProperty, value);
    }

    public bool IsUpperLayerVisible {
        get => GetValue(IsUpperLayerVisibleProperty);
        set => SetValue(IsUpperLayerVisibleProperty, value);
    }

    public float ModelDistance {
        get => GetValue(ModelDistanceProperty);
        set => SetValue(ModelDistanceProperty, value);
    }

    public SkinRenderMode RenderMode {
        get => GetValue(RenderModeProperty);
        set => SetValue(RenderModeProperty, value);
    }

    public Vector3 HeadRotation {
        get => GetValue(HeadRotationProperty);
        set => SetValue(HeadRotationProperty, value);
    }

    public Vector2 ArmRotation {
        get => GetValue(ArmRotationProperty);
        set => SetValue(ArmRotationProperty, value);
    }

    public Vector2 LegRotation {
        get => GetValue(LegRotationProperty);
        set => SetValue(LegRotationProperty, value);
    }

    public Vector2 ModelPosition {
        get => GetValue(ModelPositionProperty);
        set => SetValue(ModelPositionProperty, value);
    }

    public Vector2 ModelRotation {
        get => GetValue(ModelRotationProperty);
        set => SetValue(ModelRotationProperty, value);
    }

    public SkinViewer3D() {
        _modelDistance = ModelDistance;
    }

    public void Reset() {
        _skin?.ResetPos();
        _modelDistance = 1f;
        RequestNextFrameRendering();
    }

    public void AddDis(float x) {
        _skin?.AddDis(x);
    }

    public bool HitTest(Point point) {
        return Bounds.Contains(point);
    }

    public void ChangeSkin(Bitmap skin, Bitmap cape = default!) {
        if (skin is null && cape is null)
            return;

        if (skin != null)
            _skin!.SetSkinTex(SkinHelper.Convert(skin));

        if (cape != null)
            _skin.SetCapeTex(SkinHelper.Convert(cape));

        RequestNextFrameRendering();
    }

    public void UpdatePointerMoved(PointerType type, Vector2 point) {
        if (_skin != null && _skin.HaveSkin) {
            _skin.PointerMoved(type, point);
            RequestNextFrameRendering();
        }
    }

    public void UpdatePointerPressed(PointerType type, Vector2 point) {
        if (_skin != null && _skin.HaveSkin) {
            _skin.PointerPressed(type, point);
            RequestNextFrameRendering();
        }
    }

    public void UpdatePointerReleased(PointerType type, Vector2 point) {
        if (_skin != null && _skin.HaveSkin) {
            _skin.PointerReleased(type, point);
            RequestNextFrameRendering();
        }
    }

    protected override void OnOpenGlInit(GlInterface gl) {
        CheckError(gl);
        _skin = new OpenGLSkinViewerBase(new AvaloniaApi(gl), GlVersion.Type is GlProfileType.OpenGLES) {
            BackColor = new Vector4(0f),
            RenderMode = RenderMode,
            EnableCape = IsCapeVisible,
            Animation = IsEnableAnimation,
            EnableTop = IsUpperLayerVisible
        };

        _skin.Error += (object? _, ErrorType type) =>
            Debug.WriteLine(type.ToString());

        ChangeSkin(Skin, Cape);
        _skin.OpenGlInit();
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb) {
        int width = (int)base.Bounds.Width;
        int height = (int)base.Bounds.Height;
        if (_skin != null) {
            if (base.VisualRoot is TopLevel { RenderScaling: var renderScaling }) {
                width = (int)(base.Bounds.Width * renderScaling);
                height = (int)(base.Bounds.Height * renderScaling);
            }

            _skin.Width = width;
            _skin.Height = height;
            if (_time.Year == 1) {
                _time = DateTime.Now;
            }
            DateTime now = DateTime.Now;
            TimeSpan timeSpan = now - _time;
            _time = now;
            _skin.Tick(timeSpan.TotalSeconds);
            _skin.OpenGlRender(fb);
            CheckError(gl);
        }

        if (IsEnableAnimation && IsVisible && Opacity != 0 && Skin != null)
            RequestNextFrameRendering();
    }

    protected override void OnOpenGlDeinit(GlInterface gl) {
        _skin?.OpenGlDeinit();
        _skin = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);

        if (change.Property == SkinProperty || change.Property == CapeProperty) {
            if (_skin != null)
                ChangeSkin(Skin, Cape);
        }

        if (change.Property == RenderModeProperty) {
            _skin.RenderMode = RenderMode;
            RequestNextFrameRendering();
        }

        if (change.Property == IsEnableAnimationProperty) {
            if (_skin != null) {
                _skin.Animation = IsEnableAnimation;
                RequestNextFrameRendering();
            }
        }

        if (change.Property == IsCapeVisibleProperty) {
            if (_skin != null)
                _skin.EnableCape = IsCapeVisible;

            RequestNextFrameRendering();
        }

        if (change.Property == IsUpperLayerVisibleProperty) {
            if (_skin != null)
                _skin.EnableTop = IsUpperLayerVisible;

            RequestNextFrameRendering();
        }

        if (change.Property == HeadRotationProperty) {
            RequestNextFrameRendering();
        }

        if (change.Property == ArmRotationProperty) {
            if (_skin != null) {
                _skin.ArmRotate = new(ArmRotation.X, ArmRotation.Y, 0);
                RequestNextFrameRendering();
            }
        }

        if (change.Property == LegRotationProperty) {
            RequestNextFrameRendering();
        }

        if (change.Property == ModelDistanceProperty) {
            _skin?.AddDis(ModelDistance - _modelDistance);
            _modelDistance = ModelDistance;
        }
    }

    private static void CheckError(GlInterface gl) {
        int error;

        while ((error = gl.GetError()) != 0)
            Debug.WriteLine(error.ToString());
    }
}