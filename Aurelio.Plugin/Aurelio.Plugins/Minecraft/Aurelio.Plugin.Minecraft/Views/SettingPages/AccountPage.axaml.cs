using System.Numerics;
using Aurelio.Plugin.Minecraft.Operate;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Aurelio.Views.Main;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using LiteSkinViewer3D.Shared.Enums;
using PointerType = LiteSkinViewer3D.Shared.Enums.PointerType;

namespace Aurelio.Plugin.Minecraft.Views.SettingPages;

public partial class AccountPage : PageMixModelBase, IAurelioPage
{
    public new MinecraftPluginData Data => MinecraftPluginData.Instance;
    private bool _fl = true;
    public AccountPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
        ShortInfo = $"{MainLang.Setting} / {MainLang.Account}";
    }
    private string _shortInfo = string.Empty;

    public string ShortInfo
    {
        get => _shortInfo;
        set => SetField(ref _shortInfo, value);
    }
    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }

    private void BindingEvent()
    {
        AddAccount.Click += (_, _) => { _ = Account.AddByUi(this); };
        DelSelectedAccount.Click += (_, _) => { Account.RemoveSelected(); };
        Open3DView.Click += (_, _) =>
        {
            var tab = new TabEntry(new Render3DSkin(MinecraftPluginData.MinecraftPluginSettingEntry.UsingMinecraftAccount.Name,
                MinecraftPluginData.MinecraftPluginSettingEntry.UsingMinecraftAccount.Skin));
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

        MinecraftPluginData.MinecraftPluginSettingEntry.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(MinecraftPluginData.MinecraftPluginSettingEntry.UsingMinecraftAccount)) return;
            if (!MinecraftPluginData.Instance.IsRender3D) return;
            try
            {
                skinViewer.Skin = Public.Module.Value.Converter.Base64ToBitmap(MinecraftPluginData.MinecraftPluginSettingEntry.UsingMinecraftAccount.Skin);
                skinViewer.RenderMode = SkinRenderMode.None;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
        };
        Loaded += (_, _) =>
        {
            if (!_fl) return;
            _fl = false;
            if (!MinecraftPluginData.Instance.IsRender3D) return;
            try
            {
                skinViewer.Skin =
                    Public.Module.Value.Converter.Base64ToBitmap(MinecraftPluginData.MinecraftPluginSettingEntry
                        .UsingMinecraftAccount.Skin);
                skinViewer.RenderMode = SkinRenderMode.None;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
        };
        Data.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(MinecraftPluginData.IsRender3D)) return;
            if (!MinecraftPluginData.Instance.IsRender3D) return;
            try
            {
                skinViewer.Skin = Public.Module.Value.Converter.Base64ToBitmap(MinecraftPluginData.MinecraftPluginSettingEntry.UsingMinecraftAccount.Skin);
                skinViewer.RenderMode = SkinRenderMode.None;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
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