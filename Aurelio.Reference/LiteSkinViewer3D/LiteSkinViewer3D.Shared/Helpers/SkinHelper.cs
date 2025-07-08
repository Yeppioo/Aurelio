using LiteSkinViewer3D.Shared.Enums;
using SkiaSharp;

namespace LiteSkinViewer3D.Shared.Helpers;

/// <summary>
/// 皮肤帮助器
/// </summary>
public static class SkinHelper {
    private const int BaseSize = 64;

    /// <summary>
    /// 检测皮肤类型
    /// </summary>
    public static SkinType DetectSkin(SKBitmap image) {
        if (image == null || image.Width < 64 || image.Height < 32)
            return SkinType.Unknown;

        if (image.Width == image.Height && image.Width >= BaseSize)
            return IsSlim(image) 
                ? SkinType.Slim 
                : SkinType.Classic;

        if (image.Width == image.Height * 2)
            return SkinType.Legacy;

        return SkinType.Unknown;
    }

    /// <summary>
    /// 检查是否为 Slim 皮肤
    /// </summary>
    private static bool IsSlim(SKBitmap image) {
        int scale = image.Width / BaseSize;

        return image.CheckTransparentRegion(50, 16, 2, 4, scale)
            && image.CheckTransparentRegion(54, 20, 2, 12, scale)
            && image.CheckTransparentRegion(42, 48, 2, 4, scale)
            && image.CheckTransparentRegion(46, 52, 2, 12, scale);
    }

    /// <summary>
    /// 检查指定区域是否包含透明像素
    /// </summary>
    private static bool CheckTransparentRegion(this SKBitmap image, int x, int y, int width, int height, int scale) {
        int sx = x * scale;
        int sy = y * scale;
        int sw = width * scale;
        int sh = height * scale;

        if (sx + sw > image.Width || sy + sh > image.Height)
            return false;

        for (int dx = 0; dx < sw; dx++) {
            for (int dy = 0; dy < sh; dy++) {
                var pixel = image.GetPixel(sx + dx, sy + dy);
                if (pixel.Alpha == 0)
                    return true;
            }
        }

        return false;
    }
}
