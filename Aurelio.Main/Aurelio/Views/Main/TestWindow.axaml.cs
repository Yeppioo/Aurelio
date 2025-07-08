using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LiteSkinViewer3D.Shared.Enums;
using PointerType = LiteSkinViewer3D.Shared.Enums.PointerType;

namespace Aurelio.Views.Main;

public partial class TestWindow : Window
{
    public TestWindow()
    {
        InitializeComponent();
    }
    
    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);

        skinViewer.PointerMoved += SkinViewer_PointerMoved;
        skinViewer.PointerPressed += SkinViewer_PointerPressed;
        skinViewer.PointerReleased += SkinViewer_PointerReleased;
    }

    private void SkinViewer_PointerReleased(object? sender, PointerReleasedEventArgs e) {
        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        PointerType type = PointerType.None;
        if (po.Properties.IsLeftButtonPressed) {
            type = PointerType.PointerLeft;
        } else if (po.Properties.IsRightButtonPressed) {
            type = PointerType.PointerRight;
        }

        skinViewer.UpdatePointerReleased(type, new((float)pos.X, (float)pos.Y));

    }

    private void SkinViewer_PointerPressed(object? sender, PointerPressedEventArgs e) {
        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        PointerType type = PointerType.None;
        if (po.Properties.IsLeftButtonPressed) {
            type = PointerType.PointerLeft;
        } else if (po.Properties.IsRightButtonPressed) {
            type = PointerType.PointerRight;
        }

        skinViewer.UpdatePointerPressed(type, new((float)pos.X, (float)pos.Y));
    }

    private void SkinViewer_PointerMoved(object? sender, PointerEventArgs e) {
        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        PointerType type = PointerType.None;
        if (po.Properties.IsLeftButtonPressed) {
            type = PointerType.PointerLeft;
        } else if (po.Properties.IsRightButtonPressed) {
            type = PointerType.PointerRight;
        }

        skinViewer.UpdatePointerMoved(type, new((float)pos.X, (float)pos.Y));
    }
}