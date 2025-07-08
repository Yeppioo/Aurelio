using SkiaSharp;

namespace MinecraftSkinRender.Image;

public static class ImageHelper
{
    /// <summary>
    /// 复制像素到指定区域，同时每个像素都以填充方式填充
    /// </summary>
    /// <param name="image">写的图像</param>
    /// <param name="source">读的图像</param>
    /// <param name="x">写左边距</param>
    /// <param name="y">写又边距</param>
    /// <param name="sx">读的左边距</param>
    /// <param name="sy">读的右边距</param>
    /// <param name="swidth">读的宽度</param>
    /// <param name="sheight">读的高度</param>
    /// <param name="width">写像素宽度</param>
    /// <param name="height">写像素高度</param>
    public static void ExtractSubsetWithFillImage(this SKBitmap image, SKBitmap source, int x, int y, int sx, int sy, int swidth, int sheight, int width, int height)
    {
        for (int i = 0; i < swidth; i++)
        {
            for (int j = 0; j < sheight; j++)
            {
                image.FillImage(i * width + x, j * height + y, width, height, source.GetPixel(i + sx, j + sy));
            }
        }
    }

    /// <summary>
    /// 复制像素到指定区域，同时每个像素都以填充方式填充混合
    /// </summary>
    /// <param name="image">写的图像</param>
    /// <param name="source">读的图像</param>
    /// <param name="x">写左边距</param>
    /// <param name="y">写又边距</param>
    /// <param name="sx">读的左边距</param>
    /// <param name="sy">读的右边距</param>
    /// <param name="swidth">读的宽度</param>
    /// <param name="sheight">读的高度</param>
    /// <param name="width">写像素宽度</param>
    /// <param name="height">写像素高度</param>
    public static void ExtractSubsetWithFillImageMix(this SKBitmap image, SKBitmap source, int x, int y, int sx, int sy, int swidth, int sheight, int width, int height)
    {
        for (int i = 0; i < swidth; i++)
        {
            for (int j = 0; j < sheight; j++)
            {
                image.FillImageMix(i * width + x, j * height + y, width, height, source.GetPixel(i + sx, j + sy));
            }
        }
    }

    /// <summary>
    /// 在指定区域填充颜色
    /// </summary>
    /// <param name="image">图像</param>
    /// <param name="x">填充左边距</param>
    /// <param name="y">填充右边距</param>
    /// <param name="width">填充宽度</param>
    /// <param name="height">填充高度</param>
    /// <param name="pix">填充颜色</param>
    public static void FillImage(this SKBitmap image, int x, int y, int width, int height, SKColor pix)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                image.SetPixel(i + x, j + y, pix);
            }
        }
    }

    /// <summary>
    /// 带混合的填充
    /// </summary>
    /// <param name="image">图像</param>
    /// <param name="x">填充左边距</param>
    /// <param name="y">填充右边距</param>
    /// <param name="width">填充宽度</param>
    /// <param name="height">填充高度</param>
    /// <param name="pix">填充颜色</param>
    public static void FillImageMix(this SKBitmap image, int x, int y, int width, int height, SKColor pix)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                image.SetPixel(i + x, j + y, image.GetPixel(i + x, j + y).Mix(pix));
            }
        }
    }

    /// <summary>
    /// 提取像素并覆盖
    /// </summary>
    /// <param name="image">写的图像</param>
    /// <param name="source">读取的图像</param>
    /// <param name="x">写的X位置</param>
    /// <param name="y">写的Y位置</param>
    /// <param name="sx">读的X位置</param>
    /// <param name="sy">读的Y位置</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    public static void ExtractSubset(this SKBitmap image, SKBitmap source, int x, int y, int sx, int sy, int width, int height)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                image.SetPixel(x + i, y + j, source.GetPixel(i + sx, j + sy));
            }
        }
    }

    /// <summary>
    /// 带混合的提取像素
    /// </summary>
    /// <param name="image">写的图像</param>
    /// <param name="source">读取的图像</param>
    /// <param name="x">写的X位置</param>
    /// <param name="y">写的Y位置</param>
    /// <param name="sx">读的X位置</param>
    /// <param name="sy">读的Y位置</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    public static void ExtractSubsetMix(this SKBitmap image, SKBitmap source, int x, int y, int sx, int sy, int width, int height)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                image.SetPixel(x + i, y + j, image.GetPixel(x + i, y + j).Mix(source.GetPixel(i + sx, j + sy)));
            }
        }
    }

    /// <summary>
    /// 混合像素
    /// </summary>
    /// <param name="rgba">源</param>
    /// <param name="mix">目标</param>
    /// <returns>结果</returns>
    public static SKColor Mix(this SKColor rgba, SKColor mix)
    {
        double ap = mix.Alpha / 255;
        double dp = 1 - ap;

        var red = (byte)(mix.Red * ap + rgba.Red * dp);
        var green = (byte)(mix.Green * ap + rgba.Green * dp);
        var blue = (byte)(mix.Blue * ap + rgba.Blue * dp);
        if (rgba.Alpha == 0 && mix.Alpha == 0)
        {
            return new(red, green, blue, 0);
        }
        return new(red, green, blue);
    }

    /// <summary>
    /// 保存图片
    /// </summary>
    /// <param name="image">图片</param>
    /// <param name="file">文件名</param>
    public static void SavePng(this SKBitmap image, string file)
    {
        using var temp = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Create(file);
        temp.AsStream().CopyTo(stream);
    }

    /// <summary>
    /// 保存图片
    /// </summary>
    /// <param name="image">图片</param>
    /// <param name="file">文件名</param>
    public static void SavePng(this SKImage image, string file)
    {
        using var temp = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Create(file);
        temp.AsStream().CopyTo(stream);
    }
}
