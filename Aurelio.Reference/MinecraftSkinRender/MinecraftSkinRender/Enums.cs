namespace MinecraftSkinRender;

/// <summary>
/// 皮肤类型
/// </summary>
public enum SkinType
{
    /// <summary>
    /// 1.7旧版
    /// </summary>
    Old,
    /// <summary>
    /// 1.8新版
    /// </summary>
    New,
    /// <summary>
    /// 1.8新版纤细
    /// </summary>
    NewSlim,
    /// <summary>
    /// 未知的类型
    /// </summary>
    Unkonw
}

/// <summary>
/// 鼠标按钮
/// </summary>
public enum KeyType
{
    /// <summary>
    /// 没有按下
    /// </summary>
    None,
    /// <summary>
    /// 左键
    /// </summary>
    Left,
    /// <summary>
    /// 右键
    /// </summary>
    Right
}

/// <summary>
/// 错误
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// 未知皮肤类型
    /// </summary>
    UnknowSkinType,
    /// <summary>
    /// 没有皮肤
    /// </summary>
    SkinNotFound
}

/// <summary>
/// 当前状态
/// </summary>
public enum StateType
{
    SkinReload
}

/// <summary>
/// 模型部分
/// </summary>
public enum ModelPartType
{
    Head, Body, LeftArm, RightArm, LeftLeg, RightLeg, Cape,
    Proj, View, Model
}

/// <summary>
/// 渲染模式
/// </summary>
public enum SkinRenderType
{
    Normal, MSAA, FXAA
}