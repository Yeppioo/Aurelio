using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using LiteSkinViewer2D.Extensions;
using SkiaSharp;
using System.IO;

namespace LiteSkinViewer2D.Simple;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);

        PART_SkinTextBox.TextChanged += OnPART_SkinTextBoxTextChanged;
    }

    private void OnPART_SkinTextBoxTextChanged(object? sender, TextChangedEventArgs e) {
        if (!File.Exists(PART_SkinTextBox.Text))
            return;

        using var skin = SKBitmap.Decode(PART_SkinTextBox.Text);
        PART_SkinHeadPFPImage.Source = SideCapturer.Default.Capture(skin).ToBitmap();
        PART_SkinHeadImage.Source = HeadCapturer.Default.Capture(skin).ToBitmap();
        PART_SkinImage.Source = FullBodyCapturer.Default.Capture(skin, 8).ToBitmap();
    }
}