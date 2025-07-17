using System.Numerics;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.Operate;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.Module.Value;
using Aurelio.Public.ViewModels;
using Aurelio.Views.Main.Pages.Template;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.VisualTree;
using LiteSkinViewer3D.Shared.Enums;
using PointerType = LiteSkinViewer3D.Shared.Enums.PointerType;

namespace Aurelio.Views.Main.Pages.Instance.SubPages.SettingPages;

public partial class AccountPage : PageMixModelBase, IAurelioPage
{
    public AccountPage()
    {
        InitializeComponent();
        DataContext = Data.Instance;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }

    private void BindingEvent()
    {
        AddAccount.Click += (_, _) => { _ = Account.AddByUi(this); };
        DelSelectedAccount.Click += (_, _) => { Account.RemoveSelected(); };
        Open3DView.Click += (_, _) =>
        {
            var tab = new TabEntry(new Render3DSkin(Data.SettingEntry.UsingMinecraftAccount.Name,
                Data.SettingEntry.UsingMinecraftAccount.Skin));
            if (this.GetVisualRoot() is TabWindow window)
            {
                window.CreateTab(tab);
                return;
            }

            App.UiRoot.CreateTab(tab);
        };

        skinViewer.PointerMoved += SkinViewer_PointerMoved;
        skinViewer.PointerPressed += SkinViewer_PointerPressed;
        skinViewer.PointerReleased += SkinViewer_PointerReleased;

        Data.SettingEntry.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(Data.SettingEntry.UsingMinecraftAccount)) return;
            if (!UiProperty.Instance.IsRender3D) return;
            try
            {
                skinViewer.Skin = Converter.Base64ToBitmap(Data.SettingEntry.UsingMinecraftAccount.Skin);
                skinViewer.RenderMode = SkinRenderMode.None;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                Notice("渲染3D失败", NotificationType.Error);
            }
        };
        Data.UiProperty.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(UiProperty.IsRender3D)) return;
            if (!UiProperty.Instance.IsRender3D) return;
            try
            {
                skinViewer.Skin = Converter.Base64ToBitmap(Data.SettingEntry.UsingMinecraftAccount.Skin);
                skinViewer.RenderMode = SkinRenderMode.None;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                Notice("渲染3D失败", NotificationType.Error);
            }
        };
    }

    private void SkinViewer_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        var type = PointerType.None;
        if (po.Properties.IsLeftButtonPressed)
            type = PointerType.PointerLeft;
        else if (po.Properties.IsRightButtonPressed) type = PointerType.PointerRight;

        skinViewer.UpdatePointerReleased(type, new Vector2((float)pos.X, (float)pos.Y));
    }

    private void SkinViewer_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        var type = PointerType.None;
        if (po.Properties.IsLeftButtonPressed)
            type = PointerType.PointerLeft;
        else if (po.Properties.IsRightButtonPressed) type = PointerType.PointerRight;

        skinViewer.UpdatePointerPressed(type, new Vector2((float)pos.X, (float)pos.Y));
    }

    private void SkinViewer_PointerMoved(object? sender, PointerEventArgs e)
    {
        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        var type = PointerType.None;
        if (po.Properties.IsLeftButtonPressed)
            type = PointerType.PointerLeft;
        else if (po.Properties.IsRightButtonPressed) type = PointerType.PointerRight;

        skinViewer.UpdatePointerMoved(type, new Vector2((float)pos.X, (float)pos.Y));
    }
}