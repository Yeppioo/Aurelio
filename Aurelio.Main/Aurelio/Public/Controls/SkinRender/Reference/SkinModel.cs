using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MinecraftSkinRender;

namespace Aurelio.Public.Controls.SkinRender.Reference;

/// <summary>
/// 皮肤页面
/// </summary>
/// <param name="model"></param>

public enum MoveType
{
    LeftUp, Up, RightUp,
    Left, Right,
    LeftDown, Down, RightDown
}

public partial class SkinModel : ObservableObject
{
    public const string RotateName = "Rotate";
    public const string ScollName = "Scoll";
    public const string PosName = "Pos";
    public const string RotName = "Rot";
    public const string LoadName = "Load";
    public const string ResetName = "Reset";

    /// <summary>
    /// 皮肤类型
    /// </summary>
    public string[] SkinTypeList { get; init; } = ["Old", "New","New thin"];
    /// <summary>
    /// 皮肤关节旋转类型
    /// </summary>
    public string[] SkinRotateList { get; init; } = ["1", "2","3"];

    /// <summary>
    /// 皮肤类型
    /// </summary>
    [ObservableProperty]
    private int _type;
    /// <summary>
    /// 旋转类型
    /// </summary>
    [ObservableProperty]
    private int _rotateType;

    /// <summary>
    /// 渲染器信息
    /// </summary>
    [ObservableProperty]
    private string _info;
    /// <summary>
    /// 当前Fps
    /// </summary>
    [ObservableProperty]
    private string _nowFps;

    /// <summary>
    /// 是否有皮肤文件
    /// </summary>
    [ObservableProperty]
    private bool _haveSkin;
    /// <summary>
    /// 是否启用动画效果
    /// </summary>
    [ObservableProperty]
    private bool _enableAnimation = true;
    /// <summary>
    /// 是否启用披风
    /// </summary>
    [ObservableProperty]
    private bool _enableCape = true;
    /// <summary>
    /// 是否启用第二层
    /// </summary>
    [ObservableProperty]
    private bool _enableTop = true;
    /// <summary>
    /// 是否允许Z轴
    /// </summary>
    [ObservableProperty]
    private bool _enableZ;
    /// <summary>
    /// 是否启用FXAA
    /// </summary>
    [ObservableProperty]
    private bool _enableFXAA = false;

    /// <summary>
    /// 皮肤类型
    /// </summary>
    [ObservableProperty]
    private SkinType _steveModelType;

    /// <summary>
    /// X轴旋转
    /// </summary>
    [ObservableProperty]
    private float _rotateX;
    /// <summary>
    /// Y轴旋转
    /// </summary>
    [ObservableProperty]
    private float _rotateY;
    /// <summary>
    /// Z轴旋转
    /// </summary>
    [ObservableProperty]
    private float _rotateZ;

    /// <summary>
    /// 最小值
    /// </summary>
    [ObservableProperty]
    private float _minX;
    /// <summary>
    /// 最小值
    /// </summary>
    [ObservableProperty]
    private float _minY = -360;
    /// <summary>
    /// 最小值
    /// </summary>
    [ObservableProperty]
    private float _minZ;

    //旋转
    public Vector3 ArmRotate;
    public Vector3 LegRotate;
    public Vector3 HeadRotate;

    //旋转当前值
    public float X;
    public float Y;

    /// <summary>
    /// 是否在加载用
    /// </summary>
    private bool _load;

    public int Fps
    {
        set
        {
            NowFps = $"{value}Fps";
        }
    }

    /// <summary>
    /// 皮肤类型切换
    /// </summary>
    /// <param name="value"></param>
    partial void OnSteveModelTypeChanged(SkinType value)
    {
        _load = true;
        Type = (int)value;
        _load = false;
    }

    /// <summary>
    /// 选中的皮肤类型切换
    /// </summary>
    /// <param name="value"></param>
    partial void OnTypeChanged(int value)
    {
        if (_load)
        {
            return;
        }
        SteveModelType = (SkinType)value;
    }

    //旋转
    partial void OnRotateXChanged(float value)
    {
        switch (RotateType)
        {
            case 0:
                ArmRotate.X = value;
                break;
            case 1:
                LegRotate.X = value;
                break;
            case 2:
                HeadRotate.X = value;
                break;
            default:
                return;
        }

        OnPropertyChanged(RotateName);
    }

    partial void OnRotateYChanged(float value)
    {
        switch (RotateType)
        {
            case 0:
                ArmRotate.Y = value;
                break;
            case 1:
                LegRotate.Y = value;
                break;
            case 2:
                HeadRotate.Y = value;
                break;
            default:
                return;
        }

        OnPropertyChanged(RotateName);
    }

    partial void OnRotateZChanged(float value)
    {
        switch (RotateType)
        {
            case 0:
                ArmRotate.Z = value;
                break;
            case 1:
                LegRotate.Z = value;
                break;
            case 2:
                HeadRotate.Z = value;
                break;
            default:
                return;
        }

        OnPropertyChanged(RotateName);
    }

    partial void OnRotateTypeChanged(int value)
    {
        Vector3 rotate;
        switch (RotateType)
        {
            case 0:
                rotate = ArmRotate;
                MinX = 0;
                MinY = -360;
                MinZ = 0;
                EnableZ = false;
                break;
            case 1:
                rotate = LegRotate;
                MinX = 0;
                MinY = -360;
                MinZ = 0;
                EnableZ = false;
                break;
            case 2:
                rotate = HeadRotate;
                MinX = -360;
                MinY = -360;
                MinZ = -360;
                EnableZ = true;
                break;
            default:
                return;
        }

        RotateX = rotate.X;
        RotateY = rotate.Y;
        RotateZ = rotate.Z;
    }

    /// <summary>
    /// 移动
    /// </summary>
    /// <param name="comm"></param>
    [RelayCommand]
    public void Move(object comm)
    {
        switch (comm)
        {
            case MoveType.LeftUp:
                X = -0.05f;
                Y = 0.05f;
                break;
            case MoveType.Up:
                X = 0;
                Y = 0.05f;
                break;
            case MoveType.RightUp:
                X = 0.05f;
                Y = 0.05f;
                break;
            case MoveType.Left:
                X = -0.05f;
                Y = 0;
                break;
            case MoveType.Right:
                X = 0.05f;
                Y = 0;
                break;
            case MoveType.LeftDown:
                X = -0.05f;
                Y = -0.05f;
                break;
            case MoveType.Down:
                X = 0;
                Y = -0.05f;
                break;
            case MoveType.RightDown:
                X = 0.05f;
                Y = -0.05f;
                break;
        }
        OnPropertyChanged(PosName);
    }

    /// <summary>
    /// 旋转
    /// </summary>
    /// <param name="comm"></param>
    [RelayCommand]
    public void Rot(object comm)
    {
        switch (comm)
        {
            case MoveType.LeftUp:
                X = -10;
                Y = -10;
                break;
            case MoveType.Up:
                X = -10;
                Y = 0;
                break;
            case MoveType.RightUp:
                X = -10;
                Y = 10;
                break;
            case MoveType.Left:
                X = 0;
                Y = -10;
                break;
            case MoveType.Right:
                X = 0;
                Y = 10;
                break;
            case MoveType.LeftDown:
                X = 10;
                Y = -10;
                break;
            case MoveType.Down:
                X = 10;
                Y = 0;
                break;
            case MoveType.RightDown:
                X = 10;
                Y = 10;
                break;
        }
        OnPropertyChanged("Rot");
    }

    /// <summary>
    /// 缩放
    /// </summary>
    /// <param name="comm"></param>
    [RelayCommand]
    public void Scoll(object comm)
    {
        switch (comm)
        {
            case MoveType.Up:
                X = 0.05f;
                break;
            case MoveType.Down:
                X = -0.05f;
                break;
        }

        OnPropertyChanged(ScollName);
    }

    /// <summary>
    /// 保存皮肤
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task Save()
    {
        // var top = ColorMC.Gui.UI.Model.GetTopLevel();
        // if (top == null)
        // {
        //     return;
        // }
        // var res = await PathBinding.SaveFile(top, FileType.Skin, null);
        // if (res == true)
        // {
        //     ColorMC.Gui.UI.Model.Notify(App.Lang("ConfigEditWindow.Info9"));
        // }
        await Task.Delay(1);
    }
    /// <summary>
    /// 重置视角
    /// </summary>
    [RelayCommand]
    public void Reset()
    {
        switch (RotateType)
        {
            case 0:
                ArmRotate.X = 0;
                ArmRotate.Y = 0;
                ArmRotate.Z = 0;
                break;
            case 1:
                LegRotate.X = 0;
                LegRotate.Y = 0;
                LegRotate.Z = 0;
                break;
            case 2:
                HeadRotate.X = 0;
                HeadRotate.Y = 0;
                HeadRotate.Z = 0;
                break;
            default:
                return;
        }
        RotateX = 0;
        RotateY = 0;
        RotateZ = 0;

        OnPropertyChanged(RotateName);
    }
    /// <summary>
    /// 加载皮肤
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task Load()
    {
        // await UserBinding.LoadSkin();
        //TODO
        OnPropertyChanged(LoadName);
    }
    /// <summary>
    /// 编辑皮肤
    /// </summary>
    [RelayCommand]
    public void Edit()
    {
        // var top = ColorMC.Gui.UI.Model.GetTopLevel();
        // if (top == null)
        // {
        //     return;
        // }
        // UserBinding.EditSkin(top);
    }
    /// <summary>
    /// 重置位置
    /// </summary>
    [RelayCommand]
    public void ResetPos()
    {
        OnPropertyChanged(ResetName);
    }
}
