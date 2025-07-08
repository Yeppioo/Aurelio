using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using LiteSkinViewer3D.OpenGL;
using LiteSkinViewer3D.Shared.Enums;
using SkiaSharp;
using System.Diagnostics;
using System.Numerics;

namespace LiteSkinViewer3D.Avalonia.Controls;

public sealed class SkinRender3D : OpenGlControlBase, ICustomHitTest {
    private static readonly Vector2 DefaultModelPosition = new(0f, 0.5f);

    //public static readonly RoutedEvent<RenderFailedEventArgs> RenderFailedEvent =
    //  RoutedEvent.Register<SkinRender3D, RenderFailedEventArgs>("RenderFailed", RoutingStrategies.Bubble);

    private float _modelDistance;

    private OpenGLSkinViewerBase? _skin;

    private DateTime _time;

    public static readonly StyledProperty<bool> EnableAnimationProperty =
        AvaloniaProperty.Register<SkinRender3D, bool>("EnableAnimation", defaultValue: true);

    public static readonly StyledProperty<bool> CapeVisibilityProperty =
        AvaloniaProperty.Register<SkinRender3D, bool>("CapeVisibility", defaultValue: true);

    public static readonly StyledProperty<bool> UpperLayerVisibilityProperty =
        AvaloniaProperty.Register<SkinRender3D, bool>("UpperLayerVisibility", defaultValue: true);

    public static readonly StyledProperty<Vector3> HeadRotationProperty =
        AvaloniaProperty.Register<SkinRender3D, Vector3>("HeadRotation");

    public static readonly StyledProperty<Vector2> ArmRotationProperty =
        AvaloniaProperty.Register<SkinRender3D, Vector2>("ArmRotation", new(10, 20));

    public static readonly StyledProperty<Vector2> LegRotationProperty =
        AvaloniaProperty.Register<SkinRender3D, Vector2>("LegRotation");

    public static readonly StyledProperty<Vector2> ModelPositionProperty =
        AvaloniaProperty.Register<SkinRender3D, Vector2>("ModelPosition", DefaultModelPosition);

    public static readonly StyledProperty<float> ModelDistanceProperty =
        AvaloniaProperty.Register<SkinRender3D, float>("ModelDistance", 1f);

    public static readonly StyledProperty<Vector2> ModelRotationProperty =
        AvaloniaProperty.Register<SkinRender3D, Vector2>("ModelRotation");

    public static readonly StyledProperty<ISolidColorBrush> BackgroundProperty =
        AvaloniaProperty.Register<SkinRender3D, ISolidColorBrush>("Background");


    public bool EnableAnimation {
        get {
            return GetValue(EnableAnimationProperty);
        }
        set {
            SetValue(EnableAnimationProperty, value);
        }
    }

    public bool CapeVisibility {
        get {
            return GetValue(CapeVisibilityProperty);
        }
        set {
            SetValue(CapeVisibilityProperty, value);
        }
    }

    public bool UpperLayerVisibility {
        get {
            return GetValue(UpperLayerVisibilityProperty);
        }
        set {
            SetValue(UpperLayerVisibilityProperty, value);
        }
    }

    public Vector3 HeadRotation {
        get {
            return GetValue(HeadRotationProperty);
        }
        set {
            SetValue(HeadRotationProperty, value);
        }
    }

    public Vector2 ArmRotation {
        get {
            return GetValue(ArmRotationProperty);
        }
        set {
            SetValue(ArmRotationProperty, value);
        }
    }

    public Vector2 LegRotation {
        get {
            return GetValue(LegRotationProperty);
        }
        set {
            SetValue(LegRotationProperty, value);
        }
    }

    public Vector2 ModelPosition {
        get {
            return GetValue(ModelPositionProperty);
        }
        set {
            SetValue(ModelPositionProperty, value);
        }
    }

    public float ModelDistance {
        get {
            return GetValue(ModelDistanceProperty);
        }
        set {
            SetValue(ModelDistanceProperty, value);
        }
    }

    public Vector2 ModelRotation {
        get {
            return GetValue(ModelRotationProperty);
        }
        set {
            SetValue(ModelRotationProperty, value);
        }
    }

    public ISolidColorBrush? Background {
        get {
            return GetValue(BackgroundProperty);
        }
        set {
            SetValue(BackgroundProperty, value);
        }
    }

    public SkinRender3D() {
        _modelDistance = ModelDistance;

    }

    public bool HitTest(Point point) {
        return Bounds.Contains(point);
    }

    public void AddDis(float x) {
        _skin?.AddDis(x);
    }

    public void ChangeSkin()
    {
        var s = Convert.FromBase64String(
            "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");
        var skin = SKBitmap.Decode(new MemoryStream(s));
        // var cape = SKBitmap.Decode(@"C:\Users\wxysd\Desktop\cape.png");
        _skin.SetSkinTex(skin);
        // _skin.SetCapeTex(cape);

        RequestNextFrameRendering();
    }

    public void Reset() {
        _skin?.ResetPos();
        _modelDistance = 1f;
        RequestNextFrameRendering();
    }

    public void SetAnimation() {
        if (_skin != null) {
            _skin.Animation = EnableAnimation;
            RequestNextFrameRendering();
        }
    }

    public void UpdatePointerPressed(PointerType type, Vector2 point) {
        OpenGLSkinViewerBase skin = _skin;
        if (skin != null && skin.HaveSkin) {
            _skin.PointerPressed(type, point);
            RequestNextFrameRendering();
        }
    }

    public void UpdatePointerReleased(PointerType type, Vector2 point) {
        OpenGLSkinViewerBase skin = _skin;
        if (skin != null && skin.HaveSkin) {
            _skin.PointerReleased(type, point);
            RequestNextFrameRendering();
        }
    }

    public void UpdatePointerMoved(PointerType type, Vector2 point) {
        OpenGLSkinViewerBase skin = _skin;
        if (skin != null && skin.HaveSkin) {
            _skin.PointerMoved(type, point);
            RequestNextFrameRendering();
        }
    }

    protected override void OnOpenGlInit(GlInterface gl) {
        CheckError(gl);
        _skin = new OpenGLSkinViewerBase(new AvaloniaApi(gl), GlVersion.Type is GlProfileType.OpenGLES) {
            BackColor = new Vector4(0f),
            EnableCape = CapeVisibility,
            EnableTop = UpperLayerVisibility,
            Animation = EnableAnimation,
            RenderType = SkinRenderMode.MSAA,
        };
        _skin.Error += delegate (object? _, ErrorType type) {
            Debug.WriteLine(type.ToString());
        };

        ChangeSkin();
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

        //_skin.ArmRotate = new(0, Random.Shared.Next(0, 720), 0);
        if (EnableAnimation) {
            RequestNextFrameRendering();
        }
    }

    private void CheckError(GlInterface gl) {
        int error;
        while ((error = gl.GetError()) != 0) {
            Debug.WriteLine(error.ToString());
            //RaiseRenderFailedEvent(new Exception(error.ToString()));
        }
    }

    protected override void OnOpenGlDeinit(GlInterface gl) {
        _skin?.OpenGlDeinit();
        _skin = null;
    }

    private void OnEnableAnimationChanged() {
        SetAnimation();
    }

    private void OnCapeVisibilityChanged() {
        if (_skin != null) {
            _skin.EnableCape = CapeVisibility;
        }
        RequestNextFrameRendering();
    }

    private void OnUpperLayerVisibilityChanged() {
        if (_skin != null) {
            _skin.EnableTop = UpperLayerVisibility;
        }
        RequestNextFrameRendering();
    }

    private void OnHeadRotationChanged() {
        RequestNextFrameRendering();
    }

    private void OnArmRotationChanged() {
        if (_skin != null) {
            _skin.ArmRotate = new(ArmRotation.X, ArmRotation.Y, 0);
            RequestNextFrameRendering();
        }
    }

    private void OnLegRotationChanged() {
        RequestNextFrameRendering();
    }

    private void OnModelDistanceChanged() {
        _skin?.AddDis(ModelDistance - _modelDistance);
        _modelDistance = ModelDistance;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        if (change.Property == EnableAnimationProperty) {
            OnEnableAnimationChanged();
        }
        if (change.Property == CapeVisibilityProperty) {
            OnCapeVisibilityChanged();
        }
        if (change.Property == UpperLayerVisibilityProperty) {
            OnUpperLayerVisibilityChanged();
        }
        if (change.Property == HeadRotationProperty) {
            OnHeadRotationChanged();
        }
        if (change.Property == ArmRotationProperty) {
            OnArmRotationChanged();
        }
        if (change.Property == LegRotationProperty) {
            OnLegRotationChanged();
        }
        _ = change.Property == ModelPositionProperty;
        if (change.Property == ModelDistanceProperty) {
            OnModelDistanceChanged();
        }
        _ = change.Property == ModelRotationProperty;
        _ = change.Property == BackgroundProperty;
        base.OnPropertyChanged(change);
    }
}