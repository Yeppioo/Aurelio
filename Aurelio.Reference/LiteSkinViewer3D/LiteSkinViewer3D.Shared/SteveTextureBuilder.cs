using LiteSkinViewer3D.Shared.Enums;
using LiteSkinViewer3D.Shared.Models;

namespace LiteSkinViewer3D.Shared;

public static class SteveTextureBuilder {
    private const float SkinWidth = 64f;
    private const float SkinHeight = 64f;
    private const float CapeWidth = 64f;
    private const float CapeHeight = 32f;
    private const float LegacyHeight = 32f;

    private static readonly float[] _headTex = [
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

    private static readonly float[] _legArmTex = [
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

    private static readonly float[] _slimArmTex = [
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

    private static readonly float[] _bodyTex = [
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

    private static readonly float[] _capeTex = [
        // back
        11f, 1f, 11f, 17f, 1f, 17f, 1f, 1f,
        // front
        12f, 1f, 12f, 17f, 22f, 17f, 22f, 1f,
        // left
        11f, 1f, 11f, 17f, 12f, 17f, 12f, 1f,
        // right
        0f, 1f, 0f, 17f, 1f, 17f, 1f, 1f,
        // top
        1f, 0f, 1f, 1f, 11f, 1f, 11f, 0f,
        // bottom
        21f, 0f, 21f, 1f, 11f, 1f, 11f, 0f,
    ];

    /// <summary>
    /// 获取顶层贴图布局
    /// </summary>
    public static SteveTextureLayout GetSteveTextureTop(SkinType type) {
        var tex = new SteveTextureLayout {
            Head = GetTex(_headTex, type, 32f, 0f),
        };

        if (type != SkinType.Legacy) {
            tex.Body = GetTex(_bodyTex, type, 16f, 32f);
            var armTex = type == SkinType.Slim ? _slimArmTex : _legArmTex;
            tex.LeftArm = GetTex(armTex, type, 48f, 48f);
            tex.RightArm = GetTex(armTex, type, 40f, 32f);
            tex.LeftLeg = GetTex(_legArmTex, type, 0f, 48f);
            tex.RightLeg = GetTex(_legArmTex, type, 0f, 32f);
        }

        return tex;
    }

    /// <summary>
    /// 获取本体贴图布局
    /// </summary>
    public static SteveTextureLayout GetSteveTexture(SkinType type) {
        var tex = new SteveTextureLayout {
            Head = GetTex(_headTex, type),
            Body = GetTex(_bodyTex, type, 16f, 16f),
            Cape = GetTex(_capeTex, type, 0f, 0f, CapeWidth, CapeHeight),
        };

        if (type == SkinType.Legacy) {
            tex.LeftArm = GetTex(_legArmTex, type, 40f, 16f);
            tex.RightArm = GetTex(_legArmTex, type, 40f, 16f);
            tex.LeftLeg = GetTex(_legArmTex, type, 0f, 16f);
            tex.RightLeg = GetTex(_legArmTex, type, 0f, 16f);
        } else {
            var armTex = type == SkinType.Slim ? _slimArmTex : _legArmTex;
            tex.LeftArm = GetTex(armTex, type, 32f, 48f);
            tex.RightArm = GetTex(armTex, type, 40f, 16f);
            tex.LeftLeg = GetTex(_legArmTex, type, 0f, 16f);
            tex.RightLeg = GetTex(_legArmTex, type, 16f, 48f);
        }

        return tex;
    }

    /// <summary>
    /// 通用 UV 获取方法
    /// </summary>
    /// <param name="input">原始UV数组</param>
    /// <param name="type">皮肤类型</param>
    /// <param name="offsetU">U偏移</param>
    /// <param name="offsetV">V偏移</param>
    /// <param name="width">贴图宽度</param>
    /// <param name="height">贴图高度</param>
    public static float[] GetTex(
        float[] input,
        SkinType type,
        float offsetU = 0f,
        float offsetV = 0f,
        float width = SkinWidth,
        float height = SkinHeight) {
        var temp = new float[input.Length];
        for (int a = 0; a < input.Length; a++) {
            float value = input[a] + (a % 2 == 0 ? offsetU : offsetV);
            float divisor = (a % 2 == 0) ? width : (type == SkinType.Legacy ? LegacyHeight : height);
            temp[a] = value / divisor;
        }
        return temp;
    }
}