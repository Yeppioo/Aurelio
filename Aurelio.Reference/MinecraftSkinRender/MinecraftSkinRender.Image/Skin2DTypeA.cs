using SkiaSharp;

namespace MinecraftSkinRender.Image;

/// <summary>
/// 生成一个2D皮肤，使用简单叠加
/// </summary>
public static class Skin2DTypeA
{
    private const int Scale = 8;

    /// <summary>
    /// 创建皮肤图片
    /// </summary>
    /// <param name="image">图片</param>
    /// <param name="type">皮肤类型，空为自动</param>
    /// <returns>图片数据</returns>
    public static SKBitmap MakeSkinImage(SKBitmap image, SkinType? type = null)
    {
        using var image1 = new SKBitmap(16, 32);
        using var image2 = new SKBitmap(16 * Scale, 32 * Scale);

        //head
        image1.ExtractSubset(image, 4, 0, 8, 8, 8, 8);
        //head top
        image1.ExtractSubsetMix(image, 4, 0, 40, 8, 8, 8);
        //body
        image1.ExtractSubset(image, 4, 8, 20, 20, 8, 12);

        var skintype = type ?? SkinTypeChecker.GetTextType(image);
        if (skintype is SkinType.New or SkinType.NewSlim)
        {
            //body over
            image1.ExtractSubsetMix(image, 4, 8, 20, 36, 8, 12);
        }

        //right hand
        if (skintype is SkinType.NewSlim)
        {
            image1.ExtractSubset(image, 1, 8, 44, 20, 3, 12);
            //top
            image1.ExtractSubsetMix(image, 1, 8, 44, 36, 3, 12);
        }
        else
        {
            image1.ExtractSubset(image, 0, 8, 44, 20, 4, 12);
            if (skintype != SkinType.Old)
            {
                //top
                image1.ExtractSubsetMix(image, 0, 8, 44, 36, 4, 12);
            }
        }

        //left hand
        if (skintype == SkinType.NewSlim)
        {
            image1.ExtractSubset(image, 12, 8, 36, 52, 3, 12);
            //top
            image1.ExtractSubsetMix(image, 12, 8, 52, 52, 3, 12);
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
                        image1.SetPixel(i + 12, j + 8, image1.GetPixel(i, j).Mix(image.GetPixel(i + 44, j + 20)));
                    }
                }
            }
            else
            {
                image1.ExtractSubset(image, 12, 8, 36, 52, 4, 12);
                //top
                image1.ExtractSubsetMix(image, 12, 8, 52, 52, 4, 12);
            }
        }

        //right leg
        image1.ExtractSubset(image, 4, 20, 4, 20, 4, 12);
        if (skintype is SkinType.New or SkinType.NewSlim)
        {
            //top
            image1.ExtractSubsetMix(image, 4, 20, 4, 36, 4, 12);
        }

        //left leg
        if (skintype is SkinType.Old)
        {
            //旧版镜像
            for (int i = 3; i >= 0; i--)
            {
                for (int j = 0; j < 12; j++)
                {
                    image1.SetPixel(i + 8, j + 20, image1.GetPixel(i, j).Mix(image.GetPixel(i + 4, j + 20)));
                }
            }
        }
        else
        {
            image1.ExtractSubset(image, 8, 20, 20, 52, 4, 12);
            //top
            image1.ExtractSubsetMix(image, 8, 20, 4, 52, 4, 12);
        }

        for (int i = 0; i < 16 * Scale; i++)
        {
            for (int j = 0; j < 32 * Scale; j++)
            {
                image2.SetPixel(i, j, image1.GetPixel(i / Scale, j / Scale));
            }
        }

        return image2.Copy();
    }
}
