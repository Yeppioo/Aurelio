using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Avalonia.Input;
using PointerType = LiteSkinViewer3D.Shared.Enums.PointerType;

namespace Aurelio.Views.Main.SubPages.SettingPages;

public partial class AccountPage : PageMixModelBase, IAurelioPage
{
    private bool _loadedSkin = false;
    public AccountPage()
    {
        InitializeComponent();
        DataContext = Data.Instance;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
    }

    private void BindingEvent()
    {
        AddAccount.Click += (_, _) => { _ = Public.Module.Op.Account.AddByUi(this); };
        DelSelectedAccount.Click += (_, _) => { Public.Module.Op.Account.RemoveSelected(); };
        
        skinViewer.PointerMoved += SkinViewer_PointerMoved;
        skinViewer.PointerPressed += SkinViewer_PointerPressed;
        skinViewer.PointerReleased += SkinViewer_PointerReleased;
        
        Data.UiProperty.PropertyChanged+=(_,e)=>{
            if (e.PropertyName == nameof(Data.SettingEntry.UsingMinecraftAccount)) {
                skinViewer.ChangeSkin(Data.SettingEntry.UsingMinecraftAccount.Skin);
            }

            if (e.PropertyName != nameof(Data.UiProperty.IsEnable3DSkinRender)
                || !Data.UiProperty.IsEnable3DSkinRender || _loadedSkin) return;
            _loadedSkin = true;
            skinViewer.ChangeSkin(Data.SettingEntry.UsingMinecraftAccount.Skin);
        };
    }

    public Border RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    
    private void SkinViewer_PointerReleased(object? sender, PointerReleasedEventArgs e) {
        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        PointerType type = PointerType.None;
        if (po.Properties.IsLeftButtonPressed) {
            type = PointerType.PointerLeft;
        } else if (po.Properties.IsRightButtonPressed) {
            type = PointerType.PointerRight;
        }

        skinViewer.UpdatePointerReleased(type, new((float)pos.X, (float)pos.Y));

    }

    private void SkinViewer_PointerPressed(object? sender, PointerPressedEventArgs e) {
        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        PointerType type = PointerType.None;
        if (po.Properties.IsLeftButtonPressed) {
            type = PointerType.PointerLeft;
        } else if (po.Properties.IsRightButtonPressed) {
            type = PointerType.PointerRight;
        }

        skinViewer.UpdatePointerPressed(type, new((float)pos.X, (float)pos.Y));
    }

    private void SkinViewer_PointerMoved(object? sender, PointerEventArgs e) {
        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        PointerType type = PointerType.None;
        if (po.Properties.IsLeftButtonPressed) {
            type = PointerType.PointerLeft;
        } else if (po.Properties.IsRightButtonPressed) {
            type = PointerType.PointerRight;
        }

        skinViewer.UpdatePointerMoved(type, new((float)pos.X, (float)pos.Y));
    }
}