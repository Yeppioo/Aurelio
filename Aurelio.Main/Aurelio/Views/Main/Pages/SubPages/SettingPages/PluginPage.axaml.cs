using System.IO;
using Aurelio.Plugin.Base;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Aurelio.Views.Main.Pages.Viewers;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;

namespace Aurelio.Views.Main.Pages.SubPages.SettingPages;

public partial class PluginPage : PageMixModelBase, IAurelioPage
{
    public PluginPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        ShortInfo = $"{MainLang.Setting} / {MainLang.Download} / 已加载 {Data.LoadedPlugins.Count} 个插件";
    }
    private string _shortInfo = string.Empty;

    public string ShortInfo
    {
        get => _shortInfo;
        set => SetField(ref _shortInfo, value);
    }

    public Control BottomElement { get; set; }
    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var root = this.GetVisualRoot();
        if (((Border)sender).Tag is not LoadedPluginEntry loadedPluginEntry) return;

        if (root is TabWindow window)
        {
            window.TogglePage(null, new PluginInfo(loadedPluginEntry));
            return;
        }

        App.UiRoot.TogglePage(null, new PluginInfo(loadedPluginEntry));
    }

    private void InputElement_OnPointerPressed1(object? sender, PointerPressedEventArgs e)
    {
        var root = this.GetVisualRoot();

        if (root is TabWindow window)
        {
            window.TogglePage(null, new PluginNugetFetcher());
            return;
        }

        App.UiRoot.TogglePage(null, new PluginNugetFetcher());
    }

    private void InputElement_OnPointerPressed2(object? sender, PointerPressedEventArgs e)
    {
        App.UiRoot.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(ConfigPath.PluginFolderPath));
    }
}