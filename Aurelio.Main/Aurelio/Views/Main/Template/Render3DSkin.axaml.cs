using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Aurelio.Views.Main.Template;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
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
        PageInfo = new PageInfoEntry
        {
            Title = $"{MainLang.View3D}: {name}",
            Icon = StreamGeometry.Parse("M290.8 48.6l78.4 29.7L288 109.5 206.8 78.3l78.4-29.7c1.8-.7 3.8-.7 5.7 0zM136 92.5l0 112.2c-1.3 .4-2.6 .8-3.9 1.3l-96 36.4C14.4 250.6 0 271.5 0 294.7L0 413.9c0 22.2 13.1 42.3 33.5 51.3l96 42.2c14.4 6.3 30.7 6.3 45.1 0L288 457.5l113.5 49.9c14.4 6.3 30.7 6.3 45.1 0l96-42.2c20.3-8.9 33.5-29.1 33.5-51.3l0-119.1c0-23.3-14.4-44.1-36.1-52.4l-96-36.4c-1.3-.5-2.6-.9-3.9-1.3l0-112.2c0-23.3-14.4-44.1-36.1-52.4l-96-36.4c-12.8-4.8-26.9-4.8-39.7 0l-96 36.4C150.4 48.4 136 69.3 136 92.5zM392 210.6l-82.4 31.2 0-89.2L392 121l0 89.6zM154.8 250.9l78.4 29.7L152 311.7 70.8 280.6l78.4-29.7c1.8-.7 3.8-.7 5.7 0zm18.8 204.4l0-100.5L256 323.2l0 95.9-82.4 36.2zM421.2 250.9c1.8-.7 3.8-.7 5.7 0l78.4 29.7L424 311.7l-81.2-31.1 78.4-29.7zM523.2 421.2l-77.6 34.1 0-100.5L528 323.2l0 90.7c0 3.2-1.9 6-4.8 7.3z")
        };

        // Debug output
        System.Diagnostics.Debug.WriteLine($"Render3DSkin: Creating with name='{name}', base64 length={base64?.Length ?? 0}");
        
        Loaded+=(_,_) =>
        { 
            if(!_fl) return;
            _fl = false;
            skinViewer.ChangeSkin(_base64);
        };
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        skinViewer.PointerMoved += SkinViewer_PointerMoved;
        skinViewer.PointerPressed += SkinViewer_PointerPressed;
        skinViewer.PointerReleased += SkinViewer_PointerReleased;
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }
    public void OnClose()
    {
    }

    private void SkinViewer_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        PointerType type = PointerType.None;
        if (po.Properties.IsLeftButtonPressed)
        {
            type = PointerType.PointerLeft;
        }
        else if (po.Properties.IsRightButtonPressed)
        {
            type = PointerType.PointerRight;
        }

        skinViewer.UpdatePointerReleased(type, new((float)pos.X, (float)pos.Y));
    }

    private void SkinViewer_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        PointerType type = PointerType.None;
        if (po.Properties.IsLeftButtonPressed)
        {
            type = PointerType.PointerLeft;
        }
        else if (po.Properties.IsRightButtonPressed)
        {
            type = PointerType.PointerRight;
        }

        skinViewer.UpdatePointerPressed(type, new((float)pos.X, (float)pos.Y));
    }

    private void SkinViewer_PointerMoved(object? sender, PointerEventArgs e)
    {
        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        PointerType type = PointerType.None;
        if (po.Properties.IsLeftButtonPressed)
        {
            type = PointerType.PointerLeft;
        }
        else if (po.Properties.IsRightButtonPressed)
        {
            type = PointerType.PointerRight;
        }

        skinViewer.UpdatePointerMoved(type, new((float)pos.X, (float)pos.Y));
    }
}