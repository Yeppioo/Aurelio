using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace MinecraftSkinRender.Image;

/// <summary>
/// 创建一个2D披风图片
/// </summary>
public static class Cape2DTypaA
{
    private const int Scale = 16;
    /// <summary>
    /// 创建披风图片
    /// </summary>
    /// <param name="image">披风与鞘翅图片</param>
    /// <returns>披风图片</returns>
    public static SKBitmap MakeCapeImage(SKBitmap image)
    {
        using var image1 = new SKBitmap(10, 16);
        using var image2 = new SKBitmap(10 * Scale, 16 * Scale);

        image1.ExtractSubset(image, 0, 0, 1, 1, 10, 16);

        for (int i = 0; i < 10 * Scale; i++)
        {
            for (int j = 0; j < 16 * Scale; j++)
            {
                image2.SetPixel(i, j, image1.GetPixel(i / Scale, j / Scale));
            }
        }

        return image2.Copy();
    }
}
