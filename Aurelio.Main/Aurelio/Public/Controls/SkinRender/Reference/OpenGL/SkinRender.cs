using System.ComponentModel;
using System.IO;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using MinecraftSkinRender;
using MinecraftSkinRender.Image;
using MinecraftSkinRender.OpenGL;
using SkiaSharp;

namespace Aurelio.Public.Controls.SkinRender.Reference.OpenGL;

public class SkinRender : OpenGlControlBase, ICustomHitTest
{
    private SkinRenderOpenGL skin;
    private DateTime time;

    public SkinRender()
    {
        DataContextChanged += SkinRender_DataContextChanged;

        PointerWheelChanged += OpenGlPageControl_PointerWheelChanged;
        PointerPressed += OpenGlPageControl_PointerPressed;
        PointerReleased += OpenGlPageControl_PointerReleased;
        PointerMoved += OpenGlPageControl_PointerMoved;
    }

    private void SkinRender_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is SkinModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var model = (sender as SkinModel)!;
        if (e.PropertyName == nameof(SkinModel.SteveModelType))
        {
            skin.SkinType = model.SteveModelType;
        }
        else if (e.PropertyName == nameof(SkinModel.EnableAnimation))
        {
            skin.Animation = model.EnableAnimation;
        }
        else if (e.PropertyName == nameof(SkinModel.EnableCape))
        {
            skin.EnableCape = model.EnableCape;
        }
        else if (e.PropertyName == nameof(SkinModel.EnableTop))
        {
            skin.EnableTop = model.EnableTop;
        }
        else if (e.PropertyName == nameof(SkinModel.EnableFXAA))
        {
            skin.RenderType = model.EnableFXAA ? SkinRenderType.FXAA : SkinRenderType.Normal;
        }
        else if (e.PropertyName == SkinModel.RotateName)
        {
            skin.ArmRotate = model.ArmRotate;
            skin.LegRotate = model.LegRotate;
            skin.HeadRotate = model.HeadRotate;
        }
        else if (e.PropertyName == SkinModel.PosName)
        {
            skin.Pos(model.X, model.Y);
        }
        else if (e.PropertyName == SkinModel.ScollName)
        {
            skin.AddDis(model.X);
        }
        else if (e.PropertyName == SkinModel.RotName)
        {
            skin.Rot(model.X, model.Y);
        }
        else if (e.PropertyName == SkinModel.LoadName)
        {
            model.SteveModelType = skin.SkinType;
        }
    }

    public void ChangeSkin()
    {
        skin.SetSkinTex(GetSkinTex());
        // skin.SetCapeTex(ImageManager.CapeBitmap);
//TODO
        var model = (DataContext as SkinModel)!;
        model.Type = (int)skin.SkinType;
        model.HaveSkin = skin.HaveSkin;

        RequestNextFrameRendering();
    }

    private SKBitmap GetSkinTex()
    {
        using var stream = new MemoryStream(Convert.FromBase64String(Data.SettingEntry.UsingMinecraftAccount.Skin));
        using var skin = SKBitmap.Decode(stream);
        return Skin2DHeadTypeB.MakeHeadImage(skin);
    }
    
    public void Reset()
    {
        skin.ResetPos();

        RequestNextFrameRendering();
    }

    private void OpenGlPageControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (skin == null || !skin.HaveSkin)
        {
            return;
        }

        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        KeyType type = KeyType.None;
        if (po.Properties.IsLeftButtonPressed)
        {
            type = KeyType.Left;
        }
        else if (po.Properties.IsRightButtonPressed)
        {
            type = KeyType.Right;
        }

        skin.PointerPressed(type, new((float)pos.X, (float)pos.Y));

        RequestNextFrameRendering();
    }

    private void OpenGlPageControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (skin == null || !skin.HaveSkin)
        {
            return;
        }

        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        KeyType type = KeyType.None;
        if (po.Properties.IsLeftButtonPressed)
        {
            type = KeyType.Left;
        }
        else if (po.Properties.IsRightButtonPressed)
        {
            type = KeyType.Right;
        }

        skin.PointerReleased(type, new((float)pos.X, (float)pos.Y));

        RequestNextFrameRendering();
    }

    private void OpenGlPageControl_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (skin == null || !skin.HaveSkin)
        {
            return;
        }

        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        KeyType type = KeyType.None;
        if (po.Properties.IsLeftButtonPressed)
        {
            type = KeyType.Left;
        }
        else if (po.Properties.IsRightButtonPressed)
        {
            type = KeyType.Right;
        }

        skin.PointerMoved(type, new((float)pos.X, (float)pos.Y));

        RequestNextFrameRendering();
    }

    private void OpenGlPageControl_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (skin == null || !skin.HaveSkin)
        {
            return;
        }

        skin.PointerWheelChanged(e.Delta.Y > 0);

        RequestNextFrameRendering();
    }

    protected override unsafe void OnOpenGlInit(GlInterface gl)
    {
        var model = (DataContext as SkinModel)!;

        CheckError(gl);

        skin = new(new AvaloniaApi(gl))
        {
            IsGLES = GlVersion.Type == GlProfileType.OpenGLES,
            BackColor = new(0, 0, 0, 0),
            EnableCape = model.EnableCape,
            EnableTop = model.EnableTop,
            Animation = model.EnableAnimation
        };
        
        skin.SetSkinTex(SKBitmap.Decode(new MemoryStream(Convert.FromBase64String(Data.SettingEntry.UsingMinecraftAccount.Skin))));
        // skin.SetCapeTex(ImageManager.CapeBitmap); TODO
        skin.OpenGlInit();

        model.SteveModelType = skin.SkinType;
        model.Info = skin.Info;
    }

    protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
    {
        int x = (int)Bounds.Width;
        int y = (int)Bounds.Height;

        if (VisualRoot is TopLevel window)
        {
            var screen = window.RenderScaling;
            x = (int)(Bounds.Width * screen);
            y = (int)(Bounds.Height * screen);
        }

        skin.Width = x;
        skin.Height = y;

        if (time.Year == 1)
        {
            time = DateTime.Now;
        }

        var time1 = DateTime.Now;
        var temp = time1 - time;
        time = time1;

        skin.Tick(temp.TotalSeconds);
        skin.OpenGlRender(fb);

        var model = (DataContext as SkinModel)!;
        model.HaveSkin = skin.HaveSkin;

        CheckError(gl);
    }

    private static void CheckError(GlInterface gl)
    {
        int err;
        while ((err = gl.GetError()) != GlConsts.GL_NO_ERROR)
            Console.WriteLine(err);
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        skin.OpenGlDeinit();
    }

    public bool HitTest(Point point)
    {
        return Bounds.Contains(point);
    }
}