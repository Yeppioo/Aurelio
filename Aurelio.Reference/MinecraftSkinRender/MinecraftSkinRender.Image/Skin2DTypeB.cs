using SkiaSharp;

namespace MinecraftSkinRender.Image;

/// <summary>
/// 生成一个2D皮肤，使用复杂叠加
/// </summary>
public static class Skin2DTypeB
{
    private const int Scale = 2;

    /// <summary>
    /// 创建皮肤图片
    /// </summary>
    /// <param name="image">图片</param>
    /// <param name="type">皮肤类型，空为自动</param>
    /// <returns>图片数据</returns>
    public static SKBitmap MakeSkinImage(SKBitmap image, SkinType? type = null)
    {
        using var image1 = new SKBitmap(136, 266);
        using var image2 = new SKBitmap(136 * Scale, 266 * Scale);

        var skintype = type ?? SkinTypeChecker.GetTextType(image);

        //head
        image1.ExtractSubsetWithFillImage(image, 4 + 8 * 4, 4, 8, 8, 8, 8, 8, 8);
        //body
        image1.ExtractSubsetWithFillImage(image, 4 + 8 * 4, 4 + 8 * 8, 20, 20, 8, 12, 8, 8);

        //right hand
        if (skintype is SkinType.NewSlim)
        {
            image1.ExtractSubsetWithFillImage(image, 4 + 1 * 8, 4 + 8 * 8, 44, 20, 3, 12, 8, 8);
        }
        else
        {
            image1.ExtractSubsetWithFillImage(image, 4, 4 + 8 * 8, 44, 20, 4, 12, 8, 8);
        }

        //left hand
        if (skintype == SkinType.NewSlim)
        {
            image1.ExtractSubsetWithFillImage(image, 4 + 12 * 8, 4 + 8 * 8, 36, 52, 3, 12, 8, 8);
        }
        else
        {
            if (skintype is SkinType.Old)
            {
                //旧版镜像
                for (int i = 3; i >= 0; i--)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        var pix = image.GetPixel(i + 44, j + 20);
                        image1.FillImage(4 + 12 * 8 + i * 8, 4 + 8 * 8 + j * 8, 8, 8, pix);
                    }
                }
            }
            else
            {
                image1.ExtractSubsetWithFillImage(image, 4 + 12 * 8, 4 + 8 * 8, 36, 52, 4, 12, 8, 8);
            }
        }

        //right leg
        image1.ExtractSubsetWithFillImage(image, 4 + 4 * 8, 4 + 20 * 8, 4, 20, 4, 12, 8, 8);

        //left leg
        if (skintype is SkinType.Old)
        {
            //旧版镜像
            for (int i = 3; i >= 0; i--)
            {
                for (int j = 0; j < 12; j++)
                {
                    var pix = image.GetPixel(i + 4, j + 20);
                    image1.FillImage(4 + 8 * 8 + i * 8, 4 + 20 * 8 + j * 8, 8, 8, pix);
                }
            }
        }
        else
        {
            image1.ExtractSubsetWithFillImage(image, 4 + 8 * 8, 4 + 20 * 8, 20, 52, 4, 12, 8, 8);
        }

        if (skintype is SkinType.New or SkinType.NewSlim)
        {
            //body over
            image1.ExtractSubsetWithFillImageMix(image, 4 * 8, 8 * 8 - 2, 20, 36, 8, 12, 9, 9);
        }
        //head top
        image1.ExtractSubsetWithFillImageMix(image, 4 * 9 - 4, 0, 40, 8, 8, 8, 9, 9);
        if (skintype is SkinType.NewSlim)
        {
            //top
            image1.ExtractSubsetWithFillImageMix(image, 1 * 8 + 1, 8 * 8 + 2, 44, 36, 3, 12, 9, 9);
            //top
            image1.ExtractSubsetWithFillImageMix(image, 12 * 8 + 4, 8 * 8 + 2, 52, 52, 3, 12, 9, 9);
        }
        else if (skintype is SkinType.New)
        {
            //top
            image1.ExtractSubsetWithFillImageMix(image, 0, 8 * 8 + 2, 44, 36, 4, 12, 9, 9);
            //top
            image1.ExtractSubsetWithFillImageMix(image, 12 * 8 + 4, 8 * 8 + 2, 52, 52, 4, 12, 9, 9);
        }
        if (skintype is SkinType.New or SkinType.NewSlim)
        {
            //top
            image1.ExtractSubsetWithFillImageMix(image, 4 * 8 + 2, 20 * 8 - 2, 4, 36, 4, 12, 9, 9);
            //top
            image1.ExtractSubsetWithFillImageMix(image, 8 * 8 + 2, 20 * 8 - 2, 4, 52, 4, 12, 9, 9);
        }

        for (int i = 0; i < 136 * Scale; i++)
        {
            for (int j = 0; j < 266 * Scale; j++)
            {
                image2.SetPixel(i, j, image1.GetPixel(i / Scale, j / Scale));
            }
        }

        return image2.Copy();
    }
}
