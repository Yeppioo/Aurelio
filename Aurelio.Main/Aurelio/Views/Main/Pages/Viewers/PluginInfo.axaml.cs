using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia.Controls.Notifications;
using Avalonia.Media;

namespace Aurelio.Views.Main.Pages.Viewers;

public partial class PluginInfo : PageMixModelBase, IAurelioTabPage
{
    public LoadedPluginEntry Plugin { get; set; }

    public PluginInfo()
    {
        InitializeComponent();
    }
    private string _shortInfo = string.Empty;

    public string ShortInfo
    {
        get => _shortInfo;
        set => SetField(ref _shortInfo, value);
    }
    public PluginInfo(LoadedPluginEntry plugin)
    {
        Plugin = plugin;
        InitializeComponent();
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        PageInfo = new PageInfoEntry
        {
            Title = plugin.Plugin.Name,
            Icon = StreamGeometry.Parse("M64 128C64 110.3 78.3 96 96 96L544 96C561.7 96 576 110.3 576 128L576 160C576 177.7 561.7 192 544 192L96 192C78.3 192 64 177.7 64 160L64 128zM96 240L544 240L544 480C544 515.3 515.3 544 480 544L160 544C124.7 544 96 515.3 96 480L96 240zM248 304C234.7 304 224 314.7 224 328C224 341.3 234.7 352 248 352L392 352C405.3 352 416 341.3 416 328C416 314.7 405.3 304 392 304L248 304z")
        };
        DataContext = this;
        ShortInfo = $"{plugin.Plugin.Name} v{plugin.Plugin.Version}";
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public void OnClose()
    {
    }

    /// <summary>
    /// 删除按钮点击事件
    /// </summary>
    private async void OnDeleteButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Control control) return;

        try
        {
            await Plugin.DeletePlugin(control);
        }
        catch (Exception ex)
        {
            Logger.Error($"删除插件时发生错误: {ex.Message}");
            Notice($"删除插件失败: {ex.Message}", NotificationType.Error);
        }
    }
}