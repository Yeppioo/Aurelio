using SkiaSharp;

namespace MinecraftSkinRender.Image;

/// <summary>
/// 生成一个2D头像，使用简单叠加
/// </summary>
public static class Skin2DHeadTypeA
{
    private const int Scale = 16;

    /// <summary>
    /// 创建头像图片
    /// </summary>
    /// <param name="image">皮肤图片</param>
    /// <returns>头像图片</returns>
    public static SKBitmap MakeHeadImage(SKBitmap image)
    {
        using var image1 = new SKBitmap(8, 8);
        using var image2 = new SKBitmap(8 * Scale, 8 * Scale);

        image1.ExtractSubset(image, 0, 0, 8, 8, 8, 8);
        image1.ExtractSubsetMix(image, 0, 0, 40, 8, 8, 8);

        for (int i = 0; i < 8 * Scale; i++)
        {
            for (int j = 0; j < 8 * Scale; j++)
            {
                image2.SetPixel(i, j, image1.GetPixel(i / Scale, j / Scale));
            }
        }

        return image2.Copy();
    }
}

