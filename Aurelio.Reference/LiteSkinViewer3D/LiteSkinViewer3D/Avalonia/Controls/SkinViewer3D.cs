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
    private string _base64;

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

    public void ChangeSkin(string? base64)
    {
        if(base64==null) return;
        _base64 = base64;
        var skin = SKBitmap.Decode(new MemoryStream( Convert.FromBase64String(base64)));
        _skin.SetSkinTex(skin);
        // var cape = SKBitmap.Decode(@"C:\Users\wxysd\Desktop\cape.png");
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

        ChangeSkin(_base64);
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