namespace MinecraftSkinRender;

/// <summary>
/// 生成史蒂夫贴图UV数据
/// </summary>
public static class Steve3DTexture
{
    private static readonly float[] _headTex =
    [
        // back
        32f, 8f, 32f, 16f, 24f, 16f, 24f, 8f,
        // front
        8f, 8f, 8f, 16f, 16f, 16f, 16f, 8f,
        // left
        0f, 8f, 0f, 16f, 8f, 16f, 8f, 8f,
        // right
        16f, 8f, 16f, 16f, 24f, 16f, 24f, 8f,
        // top
        8f, 0f, 8f, 8f, 16f, 8f, 16f, 0f,
        // bottom
        24f, 0f, 24f, 8f, 16f, 8f, 16f, 0f
    ];

    private static readonly float[] _legArmTex =
    [
        // back
        12f, 4f, 12f, 16f, 16f, 16f, 16f, 4f,
        // front
        4f, 4f, 4f, 16f, 8f, 16f, 8f, 4f,
        // left
        0f, 4f, 0f, 16f, 4f, 16f, 4f, 4f,
        // right
        8f, 4f, 8f, 16f, 12f, 16f, 12f, 4f,
        // top
        4f, 0f, 4f, 4f, 8f, 4f, 8f, 0f,
        // bottom
        12f, 0f, 12f, 4f, 8f, 4f, 8f, 0f,
    ];

    private static readonly float[] _slimArmTex =
    [
        // back
        11f, 4f, 11f, 16f, 14f, 16f, 14f, 4f,
        // front
        4f, 4f, 4f, 16f, 7f, 16f, 7f, 4f,
        // left
        0f, 4f, 0f, 16f, 4f, 16f, 4f, 4f,
        // right
        7f, 4f, 7f, 16f, 10f, 16f, 10f, 4f,
        // top
        4f, 0f, 4f, 4f, 7f, 4f, 7f, 0f,
        // bottom
        10f, 0f, 10f, 4f, 7f, 4f, 7f, 0f,
    ];

    private static readonly float[] _bodyTex =
    [
        // back
        24f, 4f, 24f, 16f, 16f, 16f, 16f, 4f,
        // front
        4f, 4f, 4f, 16f, 12f, 16f, 12f, 4f,
        // left
        0f, 4f, 0f, 16f, 4f, 16f, 4f, 4f,
        // right
        12f, 4f, 12f, 16f, 16f, 16f, 16f, 4f,
        // top
        4f, 0f, 4f, 4f, 12f, 4f, 12f, 0f,
        // bottom
        20f, 0f, 20f, 4f, 12f, 4f, 12f, 0f
    ];

    private static readonly float[] _capeTex =
    [
        // back
        11f, 1f, 11f, 17f, 1f, 17f, 1f, 1f,
        // front
        12f, 1f, 12f, 17f, 22f, 17f, 22f, 1f,
        // left
        11f, 1f, 11f, 17f, 12f, 17f, 12f, 1f, 
        // right
        0f, 1f, 0f, 17f, 1f, 17f, 1f, 1f,
        // top
        1f, 0f,1f, 1f, 11f, 1f, 11f, 0f, 
        // bottom
        21f, 0f, 21f, 1f, 11f, 1f, 11f, 0f,
    ];

    /// <summary>
    /// 顶层数据
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static SteveTextureObj GetSteveTextureTop(SkinType type)
    {
        SteveTextureObj tex = new()
        {
            Head = GetTex(_headTex, type, 32f, 0f),
        };

        if (type != SkinType.Old)
        {
            tex.Body = GetTex(_bodyTex, type, 16f, 32f);
            tex.LeftArm = GetTex(type == SkinType.NewSlim ? _slimArmTex : _legArmTex, type, 48f, 48f);
            tex.RightArm = GetTex(type == SkinType.NewSlim ? _slimArmTex : _legArmTex, type, 40f, 32f);
            tex.LeftLeg = GetTex(_legArmTex, type, 0f, 48f);
            tex.RightLeg = GetTex(_legArmTex, type, 0f, 32f);
        }
        ;

        return tex;
    }

    /// <summary>
    /// 本体数据
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static SteveTextureObj GetSteveTexture(SkinType type)
    {
        SteveTextureObj tex = new()
        {
            Head = GetTex(_headTex, type),
            Body = GetTex(_bodyTex, type, 16f, 16f),
            Cape = GetCapTex(_capeTex),
        };

        if (type == SkinType.Old)
        {
            tex.LeftArm = GetTex(_legArmTex, type, 40f, 16f);
            tex.RightArm = GetTex(_legArmTex, type, 40f, 16f);
            tex.LeftLeg = GetTex(_legArmTex, type, 0f, 16f);
            tex.RightLeg = GetTex(_legArmTex, type, 0f, 16f);
        }
        else
        {
            tex.LeftArm = GetTex(type == SkinType.NewSlim ? _slimArmTex : _legArmTex, type, 32f, 48f);
            tex.RightArm = GetTex(type == SkinType.NewSlim ? _slimArmTex : _legArmTex, type, 40f, 16f);
            tex.LeftLeg = GetTex(_legArmTex, type, 0f, 16f);
            tex.RightLeg = GetTex(_legArmTex, type, 16f, 48f);
        }

        return tex;
    }

    /// <summary>
    /// 获取UV
    /// </summary>
    /// <param name="input"></param>
    /// <param name="type"></param>
    /// <param name="offsetU"></param>
    /// <param name="offsetV"></param>
    /// <returns></returns>
    public static float[] GetTex(float[] input, SkinType type,
        float offsetU = 0f,
        float offsetV = 0f)
    {
        var temp = new float[input.Length];
        for (int a = 0; a < input.Length; a++)
        {
            if (a % 2 == 0)
            {
                temp[a] = input[a] + offsetU;
            }
            else
            {
                temp[a] = input[a] + offsetV;
            }

            if (a % 2 != 0 && type == SkinType.Old)
            {
                temp[a] /= 32f;
            }
            else
            {
                temp[a] /= 64f;
            }
        }

        return temp;
    }

    /// <summary>
    /// 获取披风UV
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static float[] GetCapTex(float[] input)
    {
        var temp = new float[input.Length];
        for (int a = 0; a < input.Length; a++)
        {
            temp[a] = input[a];
            if (a % 2 == 0)
            {
                temp[a] /= 64f;
            }
            else
            {
                temp[a] /= 32f;
            }
        }

        return temp;
    }
}
