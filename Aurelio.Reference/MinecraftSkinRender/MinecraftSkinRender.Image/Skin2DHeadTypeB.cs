using SkiaSharp;

namespace MinecraftSkinRender.Image;

/// <summary>
/// 生成一个2D头像，使用复杂叠加
/// </summary>
public static class Skin2DHeadTypeB
{
    private const int Scale = 2;

    /// <summary>
    /// 创建头像图片
    /// </summary>
    /// <param name="image">皮肤图片</param>
    /// <returns>头像图片</returns>
    public static SKBitmap MakeHeadImage(SKBitmap image)
    {
        using var image1 = new SKBitmap(72, 72);
        using var image2 = new SKBitmap(72 * Scale, 72 * Scale);

        image1.ExtractSubsetWithFillImage(image, 4, 4, 8, 8, 8, 8, 8, 8);
        image1.ExtractSubsetWithFillImageMix(image, 0, 0, 40, 8, 8, 8, 9, 9);

        for (int i = 0; i < 72 * Scale; i++)
        {
            for (int j = 0; j < 72 * Scale; j++)
            {
                image2.SetPixel(i, j, image1.GetPixel(i / Scale, j / Scale));
            }
        }

        return image2.Copy();
    }
}

