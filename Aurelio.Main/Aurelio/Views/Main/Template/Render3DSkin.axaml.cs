using System.Numerics;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.Module.Value;
using Avalonia.Input;
using Avalonia.Media;
using LiteSkinViewer3D.Shared.Enums;
using PointerType = LiteSkinViewer3D.Shared.Enums.PointerType;

namespace Aurelio.Views.Main.Template;

public partial class Render3DSkin : UserControl, IAurelioTabPage
{
    private readonly string _base64;
    private bool _fl = true;

    public Render3DSkin(string name, string base64)
    {
        _base64 = base64;
        InitializeComponent();
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        PageInfo = new PageInfoEntry
        {
            Title = name,
            Icon = StreamGeometry.Parse(
                "{x:Static ui:Icons.Model3D}")
        };
        Loaded += async (_, _) =>
        {
            if (!_fl) return;
            _fl = false;
            skinViewer.Skin = Converter.Base64ToBitmap(base64);
            await Task.Delay(100);
            skinViewer.RenderMode = SkinRenderMode.None;
        };

        skinViewer.PointerMoved += SkinViewer_PointerMoved;
        skinViewer.PointerPressed += SkinViewer_PointerPressed;
        skinViewer.PointerReleased += SkinViewer_PointerReleased;
    }

    public Render3DSkin()
    {
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public void OnClose()
    {
        skinViewer.IsVisible = false;
        skinViewer.PointerMoved -= SkinViewer_PointerMoved;
        skinViewer.PointerPressed -= SkinViewer_PointerPressed;
        skinViewer.PointerReleased -= SkinViewer_PointerReleased;
    }

    private void SkinViewer_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        var type = PointerType.None;
        if (po.Properties.IsLeftButtonPressed)
            type = PointerType.PointerLeft;
        else if (po.Properties.IsRightButtonPressed) type = PointerType.PointerRight;

        skinViewer.UpdatePointerReleased(type, new Vector2((float)pos.X, (float)pos.Y));
    }

    private void SkinViewer_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        var type = PointerType.None;
        if (po.Properties.IsLeftButtonPressed)
            type = PointerType.PointerLeft;
        else if (po.Properties.IsRightButtonPressed) type = PointerType.PointerRight;

        skinViewer.UpdatePointerPressed(type, new Vector2((float)pos.X, (float)pos.Y));
    }

    private void SkinViewer_PointerMoved(object? sender, PointerEventArgs e)
    {
        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        var type = PointerType.None;
        if (po.Properties.IsLeftButtonPressed)
            type = PointerType.PointerLeft;
        else if (po.Properties.IsRightButtonPressed) type = PointerType.PointerRight;

        skinViewer.UpdatePointerMoved(type, new Vector2((float)pos.X, (float)pos.Y));
    }
}