namespace MinecraftSkinRender;

/// <summary>
/// 生成史蒂夫模型
/// </summary>
public static class Steve3DModel
{
    /// <summary>
    /// 生成一个模型
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static SteveModelObj GetSteve(SkinType type)
    {
        return new()
        {
            Head = new()
            {
                Model = CubeModel.GetSquare(),
                Point = CubeModel.GetSquareIndicies()
            },
            Body = new()
            {
                Model = CubeModel.GetSquare(multiplyZ: 0.5f, multiplyY: 1.5f),
                Point = CubeModel.GetSquareIndicies()
            },
            LeftArm = type == SkinType.NewSlim ? new()
            {
                Model = CubeModel.GetSquare(
                    multiplyX: 0.375f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f
                ),
                Point = CubeModel.GetSquareIndicies()
            } : new()
            {
                Model = CubeModel.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f
                ),
                Point = CubeModel.GetSquareIndicies()
            },
            RightArm = type == SkinType.NewSlim ? new()
            {
                Model = CubeModel.GetSquare(
                    multiplyX: 0.375f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f
                ),
                Point = CubeModel.GetSquareIndicies()
            } : new()
            {
                Model = CubeModel.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f
                ),
                Point = CubeModel.GetSquareIndicies()
            },
            LeftLeg = new()
            {
                Model = CubeModel.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f
                ),
                Point = CubeModel.GetSquareIndicies()
            },
            RightLeg = new()
            {
                Model = CubeModel.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f
                ),
                Point = CubeModel.GetSquareIndicies()
            },
            Cape = new()
            {
                Model = CubeModel.GetSquare(
                    multiplyX: 1.25f,
                    multiplyZ: 0.1f,
                    multiplyY: 2f
                ),
                Point = CubeModel.GetSquareIndicies()
            }
        };
    }

    /// <summary>
    /// 生成第二层模型
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static SteveModelObj GetSteveTop(SkinType type)
    {
        var model = new SteveModelObj
        {
            Head = new()
            {
                Model = CubeModel.GetSquare(
                    enlarge: 1.125f
                ),
                Point = CubeModel.GetSquareIndicies()
            }
        };

        if (type != SkinType.Old)
        {
            model.Body = new()
            {
                Model = CubeModel.GetSquare(
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    enlarge: 1.125f
                ),
                Point = CubeModel.GetSquareIndicies()
            };

            model.LeftArm = type == SkinType.NewSlim ? new()
            {
                Model = CubeModel.GetSquare(
                    multiplyX: 0.375f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    enlarge: 1.125f
                ),
                Point = CubeModel.GetSquareIndicies()
            } : new()
            {
                Model = CubeModel.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    enlarge: 1.125f
                ),
                Point = CubeModel.GetSquareIndicies()
            };

            model.RightArm = type == SkinType.NewSlim ? new()
            {
                Model = CubeModel.GetSquare(
                    multiplyX: 0.375f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    enlarge: 1.125f
                ),
                Point = CubeModel.GetSquareIndicies()
            } : new()
            {
                Model = CubeModel.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    enlarge: 1.125f
                ),
                Point = CubeModel.GetSquareIndicies()
            };

            model.LeftLeg = new()
            {
                Model = CubeModel.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    enlarge: 1.125f
                ),
                Point = CubeModel.GetSquareIndicies()
            };
            model.RightLeg = new()
            {
                Model = CubeModel.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    enlarge: 1.125f
                ),
                Point = CubeModel.GetSquareIndicies()
            };
        }

        return model;
    }
}
