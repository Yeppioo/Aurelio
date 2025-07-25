﻿using Avalonia.Media.Imaging;
using SkiaSharp;

namespace LiteSkinViewer2D.Extensions;

public static unsafe class SkiaExtension
{
    /// <summary>
    ///     获取像素指针和每行像素数
    /// </summary>
    public static bool TryGetPixelPointer(this SKBitmap bmp, out uint* ptr, out int rowDiv4)
    {
        ptr = null;
        rowDiv4 = 0;
        var pm = bmp.PeekPixels();
        if (pm == null)
            return false;

        ptr = (uint*)pm.GetPixels().ToPointer();
        rowDiv4 = pm.RowBytes >> 2;
        return true;
    }

    /// <summary>
    ///     按块复制（不混合），blockWidth/Height 是放大倍数
    /// </summary>
    public static void CopyBlock(this SKBitmap src, SKBitmap dst,
        int sx, int sy, int sw, int sh,
        int dx, int dy, int blockWidth, int blockHeight)
    {
        if (!src.TryGetPixelPointer(out var sPtr, out var sRow))
            return;

        if (!dst.TryGetPixelPointer(out var dPtr, out var dRow))
            return;

        for (var j = 0; j < sh; j++)
        {
            var sLine = sPtr + (sy + j) * sRow + sx;
            var baseY = dy + j * blockHeight;

            for (var i = 0; i < sw; i++)
            {
                var c = sLine[i];
                var baseX = dx + i * blockWidth;
                for (var py = 0; py < blockHeight; py++)
                {
                    var dLine = dPtr + (baseY + py) * dRow + baseX;

                    for (var px = 0; px < blockWidth; px++)
                        dLine[px] = c;
                }
            }
        }
    }

    public static void CopyBlockBlend(this SKBitmap src, SKBitmap dst,
        int sx, int sy, int sw, int sh,
        int dx, int dy, int blockWidth, int blockHeight)
    {
        if (!src.TryGetPixelPointer(out var sPtr, out var sRow))
            return;

        if (!dst.TryGetPixelPointer(out var dPtr, out var dRow))
            return;

        for (var j = 0; j < sh; j++)
        {
            var sLine = sPtr + (sy + j) * sRow + sx;
            var baseY = dy + j * blockHeight;
            for (var i = 0; i < sw; i++)
            {
                var sc = sLine[i];
                var sa = (byte)(sc >> 24);
                if (sa is 0)
                    continue;

                float af = sa / 255f, bf = 1f - af;
                var baseX = dx + i * blockWidth;

                for (var py = 0; py < blockHeight; py++)
                {
                    var dLine = dPtr + (baseY + py) * dRow + baseX;
                    for (var px = 0; px < blockWidth; px++)
                    {
                        var dc = dLine[px];
                        byte dr = (byte)(dc >> 16), dg = (byte)(dc >> 8), db = (byte)dc, da = (byte)(dc >> 24);
                        byte sr = (byte)(sc >> 16), sg = (byte)(sc >> 8), sb = (byte)sc;
                        var rr = (byte)(sr * af + dr * bf);
                        var rg = (byte)(sg * af + dg * bf);
                        var rb = (byte)(sb * af + db * bf);
                        var ra = (byte)(sa + da * bf);
                        dLine[px] = (uint)((ra << 24) | (rr << 16) | (rg << 8) | rb);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     水平镜像复制一块区域
    /// </summary>
    public static void CopyBlockMirror(this SKBitmap src, SKBitmap dst,
        int sx, int sy, int sw, int sh,
        int dx, int dy, int blockSize)
    {
        if (!src.TryGetPixelPointer(out var sPtr, out var sRow))
            return;

        if (!dst.TryGetPixelPointer(out var dPtr, out var dRow))
            return;

        for (var j = 0; j < sh; j++)
        {
            var baseY = dy + j * blockSize;
            for (var i = 0; i < sw; i++)
            {
                var c = sPtr[(sy + j) * sRow + (sx + sw - 1 - i)];
                var baseX = dx + i * blockSize;

                for (var py = 0; py < blockSize; py++)
                {
                    var dLine = dPtr + (baseY + py) * dRow + baseX;
                    for (var px = 0; px < blockSize; px++)
                        dLine[px] = c;
                }
            }
        }
    }

    /// <summary>
    ///     同步拉伸缩放，nearest-neighbor
    /// </summary>
    public static SKBitmap ResizeNearest(this SKBitmap src, int scale)
    {
        int w = src.Width, h = src.Height;
        var dst = new SKBitmap(w * scale, h * scale);
        if (!src.TryGetPixelPointer(out var sPtr, out var sRow))
            return dst;

        if (!dst.TryGetPixelPointer(out var dPtr, out var dRow))
            return dst;

        for (var y = 0; y < h * scale; y++)
        {
            var dLine = dPtr + y * dRow;
            var sLine = sPtr + y / scale * sRow;

            for (var x = 0; x < w * scale; x++) dLine[x] = sLine[x / scale];
        }

        return dst;
    }

    public static Bitmap ToBitmap(this SKBitmap img, int preferredWidth = 256)
    {
        using var sKData = img.Encode(SKEncodedImageFormat.Png, 100);
        return Bitmap.DecodeToWidth(sKData.AsStream(), preferredWidth);
    }
}