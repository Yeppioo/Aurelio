using SkiaSharp;

namespace MinecraftSkinRender.Image;

/// <summary>
/// 3D头像生成，生成类似游戏内的
/// </summary>
public static class Skin3DHeadTypeA
{
    private static readonly SKPoint3[] s_cubeVertices =
    [
        // Front face
        new SKPoint3(-1, -1,  1),
        new SKPoint3( 1, -1,  1),
        new SKPoint3( 1,  1,  1),
        new SKPoint3(-1,  1,  1),
        // Back face
        new SKPoint3(-1, -1, -1),
        new SKPoint3( 1, -1, -1),
        new SKPoint3( 1,  1, -1),
        new SKPoint3(-1,  1, -1),
        // Front face (Top)
        new SKPoint3(-1 * 1.125f, -1 * 1.125f,  1 * 1.125f),
        new SKPoint3( 1 * 1.125f, -1 * 1.125f,  1 * 1.125f),
        new SKPoint3( 1 * 1.125f,  1 * 1.125f,  1 * 1.125f),
        new SKPoint3(-1 * 1.125f,  1 * 1.125f,  1 * 1.125f),
        // Back face (Top)                          
        new SKPoint3(-1 * 1.125f, -1 * 1.125f, -1 * 1.125f),
        new SKPoint3( 1 * 1.125f, -1 * 1.125f, -1 * 1.125f),
        new SKPoint3( 1 * 1.125f,  1 * 1.125f, -1 * 1.125f),
        new SKPoint3(-1 * 1.125f,  1 * 1.125f, -1 * 1.125f)
    ];

    // 定义立方体索引
    private static readonly ushort[] s_cubeIndices =
    [
        8, 12, 15, 11, // Back face (Top)
        8, 12, 13, 9, // Bottom face (Top)
        8, 9, 10, 11, // Right face (Top)
        0, 4, 7, 3, // Back face
        0, 4, 5, 1, // Bottom face
        0, 1, 2, 3, // Right face
        3, 7, 6, 2, // Top face
        4, 5, 6, 7, // Left face
        1, 5, 6, 2, // Front face
        11, 15, 14, 10, // Top face (Top)
        12, 13, 14, 15, // Left face (Top)
        9, 13, 14, 10, // Front face (Top)
    ];

    // Define the colors for each face
    private static readonly SKRectI[] s_facePos =
    [
        new SKRectI(56, 8, 64, 16), // Back face (Top)
        new SKRectI(48, 0, 56, 8),  // Bottom face (Top)
        new SKRectI(48, 8, 56, 16), // Right face (Top)
        new SKRectI(24, 8, 32, 16), // Back face
        new SKRectI(16, 0, 24, 8),  // Bottom face
        new SKRectI(16, 8, 24, 16), // Right face
        new SKRectI(8, 0, 16, 8),   // Top face
        new SKRectI(0, 8, 8, 16),   // Left face
        new SKRectI(8, 8, 16, 16),  // Front face
        new SKRectI(40, 0, 48, 8),  // Top face (Top)
        new SKRectI(32, 8, 40, 16), // Left face (Top)
        new SKRectI(40, 8, 48, 16), // Front face (Top)
    ];

    // 定义原始图像的四个顶点
    private static readonly SKPoint[] s_sourceVertices =
    [
        // Back face
        new SKPoint(0, 1),
        new SKPoint(1, 1),
        new SKPoint(1, 0),
        new SKPoint(0, 0),
        // Bottom face
        new SKPoint(1, 0),
        new SKPoint(0, 0),
        new SKPoint(0, 1),
        new SKPoint(1, 1),
        // Right face
        new SKPoint(1, 1),
        new SKPoint(0, 1),
        new SKPoint(0, 0),
        new SKPoint(1, 0),
        // Back face
        new SKPoint(0, 1),
        new SKPoint(1, 1),
        new SKPoint(1, 0),
        new SKPoint(0, 0),
        // Bottom face
        new SKPoint(1, 0),
        new SKPoint(0, 0),
        new SKPoint(0, 1),
        new SKPoint(1, 1),
        // Right face
        new SKPoint(1, 1),
        new SKPoint(0, 1),
        new SKPoint(0, 0),
        new SKPoint(1, 0),
        // Top face
        new SKPoint(1, 0),
        new SKPoint(0, 0),
        new SKPoint(0, 1),
        new SKPoint(1, 1),
        // Left face
        new SKPoint(0, 1),
        new SKPoint(1, 1),
        new SKPoint(1, 0),
        new SKPoint(0, 0),
        // Front face
        new SKPoint(1, 1),
        new SKPoint(0, 1),
        new SKPoint(0, 0),
        new SKPoint(1, 0),
        // Top face
        new SKPoint(1, 0),
        new SKPoint(0, 0),
        new SKPoint(0, 1),
        new SKPoint(1, 1),
        // Left face
        new SKPoint(0, 1),
        new SKPoint(1, 1),
        new SKPoint(1, 0),
        new SKPoint(0, 0),
        // Front face
        new SKPoint(1, 1),
        new SKPoint(0, 1),
        new SKPoint(0, 0),
        new SKPoint(1, 0)
    ];

    public static SKImage MakeHeadImage(SKBitmap skin)
    {
        // 创建绘图表面
        int width = 400;
        int height = 400;
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        // 绘制头部
        DrawHead3D(canvas, skin);

        // 保存结果到文件
        return surface.Snapshot();
    }

    private static void DrawHead3D(SKCanvas canvas, SKBitmap texture)
    {
        var transform = CreateTransformMatrix();

        for (int i = 0; i < s_cubeIndices.Length / 4; i++)
        {
            DrawTexturedFace(canvas, texture, transform, i);
        }
    }

    // 创建3D变换矩阵
    private static SKMatrix44 CreateTransformMatrix()
    {
        var transform = SKMatrix44.CreateIdentity();

        // 平移图像到原点
        var translateToOrigin = SKMatrix44.CreateIdentity();
        translateToOrigin.SetTranslate(-100, -100, 0);

        //// 旋转矩阵
        var rotationX = CreateRotationMatrix(30, 1, 0, 0);
        var rotationY = CreateRotationMatrix(45, 0, 1, 0);

        // 将旋转矩阵相乘
        transform.PreConcat(translateToOrigin);
        transform.PreConcat(rotationX);
        transform.PreConcat(rotationY);

        // 缩放
        var scale = SKMatrix44.CreateIdentity();
        scale.SetScale(110, -110, 110);
        transform.PreConcat(scale);

        // Step 4: 平移图像到画布中心
        var translateToCenter = SKMatrix44.CreateIdentity();
        translateToCenter.SetTranslate(3.83f, -1.63f, 0);
        transform.PreConcat(translateToCenter);

        return transform;
    }

    // 创建旋转矩阵
    private static SKMatrix44 CreateRotationMatrix(float degrees, float x, float y, float z)
    {
        var radians = degrees * (float)Math.PI / 180.0f;
        var cos = (float)Math.Cos(radians);
        var sin = (float)Math.Sin(radians);
        var oneMinusCos = 1.0f - cos;

        var rotation = SKMatrix44.CreateIdentity();
        rotation[0, 0] = cos + x * x * oneMinusCos;
        rotation[0, 1] = x * y * oneMinusCos - z * sin;
        rotation[0, 2] = x * z * oneMinusCos + y * sin;
        rotation[1, 0] = y * x * oneMinusCos + z * sin;
        rotation[1, 1] = cos + y * y * oneMinusCos;
        rotation[1, 2] = y * z * oneMinusCos - x * sin;
        rotation[2, 0] = z * x * oneMinusCos - y * sin;
        rotation[2, 1] = z * y * oneMinusCos + x * sin;
        rotation[2, 2] = cos + z * z * oneMinusCos;

        return rotation;
    }


    // 应用3D变换并投影到2D
    private static SKPoint Project(SKMatrix44 mat, SKPoint3 v)
    {
        // 创建一个4D向量
        float[] vec = [v.X, v.Y, v.Z, 1];
        float[] result = new float[4];

        // 执行矩阵乘法
        mat.MapScalars(vec, result);

        // 进行透视除法
        if (result[3] != 0)
        {
            result[0] /= result[3];
            result[1] /= result[3];
            result[2] /= result[3];
        }

        return new SKPoint(result[0], result[1]);
    }

    /// <summary>
    /// 绘制一个面
    /// </summary>
    /// <param name="canvas">画布</param>
    /// <param name="texture">贴图</param>
    /// <param name="transform">变换</param>
    /// <param name="index">位于第几个面</param>
    private static void DrawTexturedFace(SKCanvas canvas, SKBitmap texture, SKMatrix44 transform, int index)
    {
        var vertices = new SKPoint[4];
        var texCoords = new SKPoint[4];
        using var sourceBitmap = new SKBitmap(8, 8);
        texture.ExtractSubset(sourceBitmap, s_facePos[index]);

        index *= 4;
        for (int j = 0; j < 4; ++j)
        {
            vertices[j] = Project(transform, s_cubeVertices[s_cubeIndices[index + j]]);
            texCoords[j] = new SKPoint(s_sourceVertices[index + j].X * sourceBitmap.Width, s_sourceVertices[index + j].Y * sourceBitmap.Height);
        }

        var skVertices = SKVertices.CreateCopy(SKVertexMode.TriangleFan, vertices, texCoords, null);

        using var paint = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateBitmap(sourceBitmap, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp)
        };

        // 绘制带纹理的面
        canvas.DrawVertices(skVertices, SKBlendMode.SrcOver, paint);
    }
}
