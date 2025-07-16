using System.Numerics;
using LiteSkinViewer3D.Shared.Enums;
using LiteSkinViewer3D.Shared.Extensions;
using LiteSkinViewer3D.Shared.Models;

namespace LiteSkinViewer3D.Shared;

public static class SteveModelFactory
{
    /// <summary>
    ///     构建基础模型（第一层）
    /// </summary>
    public static SteveMeshModel CreateBaseModel(SkinType type)
    {
        return new SteveMeshModel
        {
            Head = CreateCube(1.0f, 1.0f, 1.0f),
            Body = CreateCube(1.0f, 1.5f, 0.5f),
            LeftArm = CreateCube(GetArmScale(type), 1.5f),
            RightArm = CreateCube(GetArmScale(type), 1.5f),
            LeftLeg = CreateCube(scaleY: 1.5f, scaleZ: 0.5f),
            RightLeg = CreateCube(scaleY: 1.5f, scaleZ: 0.5f),
            Cape = CreateCube(1.25f, 2f, 0.1f)
        };
    }

    /// <summary>
    ///     构建第二层叠加模型（如帽子、袖子），针对非 Old 版皮肤
    /// </summary>
    public static SteveMeshModel CreateOverlayModel(SkinType type)
    {
        var model = new SteveMeshModel
        {
            Head = CreateCube(1.0f, 1.0f, 1.0f, 1.125f)
        };

        if (type == SkinType.Legacy)
            return model;

        var armX = GetArmScale(type);
        model.Body = CreateCube(scaleY: 1.5f, scaleZ: 0.5f, enlarge: 1.125f);
        model.LeftArm = CreateCube(armX, 1.5f, 0.5f, 1.125f);
        model.RightArm = CreateCube(armX, 1.5f, 0.5f, 1.125f);
        model.LeftLeg = CreateCube(scaleY: 1.5f, scaleZ: 0.5f, enlarge: 1.125f);
        model.RightLeg = CreateCube(scaleY: 1.5f, scaleZ: 0.5f, enlarge: 1.125f);

        return model;
    }

    /// <summary>
    ///     根据 SkinType 获取手臂宽度（Slim 模型细手臂）
    /// </summary>
    private static float GetArmScale(SkinType type)
    {
        return type == SkinType.Slim ? 0.375f : 0.5f;
    }

    /// <summary>
    ///     构造方块模型部件（Cube）
    /// </summary>
    private static CubeItemModel CreateCube(float scaleX = 0.5f, float scaleY = 1f, float scaleZ = 0.5f,
        float enlarge = 1.0f)
    {
        Vector3 scale = new(
            scaleX * enlarge,
            scaleY * enlarge,
            scaleZ * enlarge
        );

        var vertices = Cube.GetTransformedVertices(scale, Vector3.Zero);
        return new CubeItemModel
        {
            Indices = Cube.GetIndices(),
            Vertices = vertices.ToFloatArray()
        };
    }
}