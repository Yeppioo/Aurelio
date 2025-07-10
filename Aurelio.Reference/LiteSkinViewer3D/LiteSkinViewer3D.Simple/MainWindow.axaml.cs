using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LiteSkinViewer3D.Shared.Enums;
using System.IO;
using PointerType = LiteSkinViewer3D.Shared.Enums.PointerType;

namespace LiteSkinViewer3D.Simple;
public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);

        PART_DoubleRenderCheckBox.IsCheckedChanged += OnPART_DoubleRenderCheckBoxIsCheckedChanged;
        PART_CapeRenderCheckBox.IsCheckedChanged += OnPART_CapeRenderCheckBoxIsCheckedChanged;
        PART_AniamtionCheckBox.IsCheckedChanged += OnPART_AniamtionCheckBoxIsCheckedChanged;
        PART_AAComboBox.SelectionChanged += OnPART_AAComboBoxSelectionChanged;
        PART_SkinTextBox.TextChanged += OnPART_SkinTextBoxTextChanged;
        PART_CapeTextBox.TextChanged += OnPART_CapeTextBoxTextChanged;

        skinViewer.PointerMoved += SkinViewer_PointerMoved;
        skinViewer.PointerPressed += SkinViewer_PointerPressed;
        skinViewer.PointerReleased += SkinViewer_PointerReleased;
    }

    private void OnPART_DoubleRenderCheckBoxIsCheckedChanged(object? sender, RoutedEventArgs e) {
        skinViewer.IsUpperLayerVisible = PART_DoubleRenderCheckBox.IsChecked!.Value;
    }

    private void OnPART_CapeRenderCheckBoxIsCheckedChanged(object? sender, RoutedEventArgs e) {
        skinViewer.IsCapeVisible = PART_CapeRenderCheckBox.IsChecked!.Value;
    }

    private void OnPART_AniamtionCheckBoxIsCheckedChanged(object? sender, RoutedEventArgs e) {
        skinViewer.IsEnableAnimation = PART_AniamtionCheckBox.IsChecked!.Value;
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

    private void OnPART_SkinTextBoxTextChanged(object? sender, TextChangedEventArgs e) {
        if (!File.Exists(PART_SkinTextBox.Text))
            return;

        skinViewer.Skin = new(PART_SkinTextBox.Text);
    }

    private void OnPART_CapeTextBoxTextChanged(object? sender, TextChangedEventArgs e) {
        if (!File.Exists(PART_CapeTextBox.Text))
            return;

        skinViewer.Cape = new(PART_CapeTextBox.Text);
    }

    private void OnPART_AAComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        skinViewer.RenderMode = PART_AAComboBox.SelectedIndex switch {
            0 => SkinRenderMode.None,
            1 => SkinRenderMode.FXAA,
            2 => SkinRenderMode.MSAA,
            _ => SkinRenderMode.None
        };
    }
}