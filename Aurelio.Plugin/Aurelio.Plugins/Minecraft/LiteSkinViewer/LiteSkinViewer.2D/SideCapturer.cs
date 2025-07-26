using LiteSkinViewer.Shared.Interfaces;
using SkiaSharp;

namespace LiteSkinViewer2D;

public sealed class SideCapturer : ICapturer {
    public static readonly SideCapturer Default = new();

    public SKBitmap Capture(SKBitmap image, int scale = 8) {
        const int BASE_SIZE = 20;
        int outSize = BASE_SIZE * scale;
        var result = new SKBitmap(outSize, outSize);
        using var canvas = new SKCanvas(result);
        canvas.Clear(SKColors.Transparent);

        bool isLegacy = image.Height == 32;
        (SKRect src, SKRect dst)[] bottomLayers = isLegacy
            ? [
                (SKRect.Create( 8,  9, 7, 7), SKRect.Create( 8,  4, 7, 7)),
                (SKRect.Create( 5,  9, 3, 7), SKRect.Create( 5,  4, 3, 7)),
                (SKRect.Create(44, 20, 3, 7), SKRect.Create(12, 13, 3, 7)),
                (SKRect.Create(21, 20, 6, 1), SKRect.Create( 7, 11, 6, 1)),
                (SKRect.Create(20, 21, 8, 8), SKRect.Create( 6, 12, 8, 8)),
                (SKRect.Create(44, 20, 3, 7), SKRect.Create( 5, 13, 3, 7))]
            : [
                (SKRect.Create( 8,  9, 7, 7), SKRect.Create( 8,  4, 7, 7)),
                (SKRect.Create( 5,  9, 3, 7), SKRect.Create( 5,  4, 3, 7)),
                (SKRect.Create(36, 52, 3, 7), SKRect.Create(12, 13, 3, 7)),
                (SKRect.Create(21, 20, 6, 1), SKRect.Create( 7, 11, 6, 1)),
                (SKRect.Create(20, 21, 8, 8), SKRect.Create( 6, 12, 8, 8)),
                (SKRect.Create(44, 20, 3, 7), SKRect.Create( 5, 13, 3, 7))];

        (SKRect src, SKRect dst)[] topLayers = isLegacy
            ? [
                (SKRect.Create(40,  9, 7, 7), SKRect.Create( 8,  4, 7, 7)),
                (SKRect.Create(33,  9, 3, 7), SKRect.Create( 5,  4, 3, 7))]
            : [
                (SKRect.Create(40,  9, 7, 7), SKRect.Create( 8,  4, 7, 7)),
                (SKRect.Create(33,  9, 3, 7), SKRect.Create( 5,  4, 3, 7)),
                (SKRect.Create(52, 52, 3, 7), SKRect.Create(12, 13, 3, 7)),
                (SKRect.Create(52, 36, 3, 7), SKRect.Create( 5, 13, 3, 7)),
                (SKRect.Create(20, 37, 8, 8), SKRect.Create( 6, 12, 8, 8)),
                (SKRect.Create(21, 36, 6, 1), SKRect.Create( 7, 11, 6, 1))];

        foreach (var (src, dst) in bottomLayers) {
            var dstScaled = SKRect.Create(
                dst.Left * scale,
                dst.Top * scale,
                dst.Width * scale,
                dst.Height * scale
            );

            canvas.DrawBitmap(image, src, dstScaled);
        }

        foreach (var (src, dst) in topLayers) {
            var dstScaled = SKRect.Create(
                dst.Left * scale,
                dst.Top * scale,
                dst.Width * scale,
                dst.Height * scale
            );

            canvas.DrawBitmap(image, src, dstScaled);
        }

        return result;
    }
}