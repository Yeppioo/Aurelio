using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Gui.UI;
using SukiUI.Controls.Experimental.DesktopEnvironment;

namespace Aurelio.Public.Controls.SkinRender;

/// <summary>
/// Ƥ����ʾ����
/// </summary>
public partial class SkinControl : UserControl
{
    /// <summary>
    /// ��Ⱦ��ʱ��
    /// </summary>
    private FpsTimer _renderTimer;

    private readonly SkinSideControl _side = new();

    public SkinControl()
    {
        InitializeComponent();


        // DataContextChanged += SkinControl_DataContextChanged;
        SidePanel3.PointerPressed += SidePanel3_PointerPressed;
    }

    private void SidePanel3_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        SidePanel3.IsVisible = false;
    }

    // private void MinModelChange()
    // {
    //     if (DataContext is Reference.SkinModel model)
    //     {
    //         if (model.MinMode)
    //         {
    //             Decorator1.IsVisible = false;
    //             Decorator1.Child = null;
    //             Decorator2.Child = _side;
    //             model.Model.SetChoiseCall(WindowId, DisplaySide);
    //             model.Model.SetChoiseContent(WindowId, App.Lang("SkinWindow.Text10"));
    //         }
    //         else
    //         {
    //             SidePanel3.IsVisible = false;
    //             Decorator1.IsVisible = true;
    //             Decorator2.Child = null;
    //             Decorator1.Child = _side;
    //             model.Model.RemoveChoiseData(WindowId);
    //         }
    //     }
    // }

    private void DisplaySide()
    {
        SidePanel3.IsVisible = !SidePanel3.IsVisible;
    }

    // private void SkinControl_DataContextChanged(object? sender, EventArgs e)
    // {
    //     if (DataContext is Reference.SkinModel model)
    //     {
    //         model.PropertyChanged += Model_PropertyChanged1;
    //     }
    // }

    // private void Model_PropertyChanged1(object? sender, PropertyChangedEventArgs e)
    // {
    //     if (e.PropertyName == TopModel.MinModeName)
    //     {
    //         MinModelChange();
    //     }
    //     else if (e.PropertyName == Reference.SkinModel.ResetName)
    //     {
    //         Skin.Reset();
    //     }
    // }

    // public override async Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
    // {
    //     if (e.Key == Key.F5)
    //     {
    //         await (DataContext as Reference.SkinModel)!.Load();
    //
    //         return true;
    //     }
    //
    //     return false;
    // }
    //
    // public override void ControlStateChange(WindowState state)
    // {
    //     _renderTimer.Pause = state != WindowState.Minimized;
    // }
    //
    // public override void Opened()
    // {
    //     _renderTimer = new(Skin)
    //     {
    //         FpsTick = (fps) =>
    //         {
    //             Dispatcher.UIThread.Post(() =>
    //             {
    //                 if (DataContext is not Reference.SkinModel model)
    //                 {
    //                     return;
    //                 }
    //                 model.Fps = fps;
    //             });
    //         }
    //     };
    //     MinModelChange();
    // }

    // public override void Closed()
    // {
    //     ImageManager.SkinChange -= SkinChange;
    //
    //     _renderTimer.Close();
    //
    //     WindowManager.SkinWindow = null;
    // }
    //
    // protected override TopModel GenModel(BaseModel model)
    // {
    //     var amodel = new Reference.SkinModel(model);
    //     amodel.PropertyChanged += Model_PropertyChanged;
    //     return amodel;
    // }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Reference.SkinModel.HaveSkin))
        {
            if ((DataContext as Reference.SkinModel)!.HaveSkin)
            {
                _renderTimer.Pause = false;
            }
            else
            {
                _renderTimer.Pause = true;
            }
        }
    }

    private void SkinChange()
    {
        Skin.ChangeSkin();
    }
}
