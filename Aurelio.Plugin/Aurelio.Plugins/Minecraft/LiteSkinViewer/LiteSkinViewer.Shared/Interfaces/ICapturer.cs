using SkiaSharp;

namespace LiteSkinViewer.Shared.Interfaces;

public interface ICapturer {
    SKBitmap? Capture(SKBitmap image, int scale = 8);
}