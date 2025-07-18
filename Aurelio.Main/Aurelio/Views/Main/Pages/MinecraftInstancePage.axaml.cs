using System.IO;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum.Minecraft;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Aurelio.Views.Main.Pages.SubPages.MinecraftInstancePages;
using Avalonia.Platform.Storage;
using Ursa.Controls;
using Calculator = Aurelio.Public.Module.Service.Minecraft.Calculator;

namespace Aurelio.Views.Main.Pages;

public partial class MinecraftInstancePage : PageMixModelBase, IAurelioTabPage
{
    private bool _fl = true;
    private SelectionListItem _selectedItem;

    public MinecraftInstancePage(RecordMinecraftEntry entry)
    {
        Entry = entry;
        OverViewPage = new OverViewPage(Entry);
        ModPage = new ModPage(Entry.MlEntry);
        ResourcePackPage = new ResourcePackPage(Entry.MlEntry);
        SavePage = new SavePage(Entry.MlEntry);
        ShaderPackPage = new ShaderPackPage(Entry.MlEntry);
        ScreenshotPage = new ScreenshotPage(Entry.MlEntry);
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        PageInfo = new PageInfoEntry
        {
            Title = Entry.Id,
            Icon = Icons.Seedling
        };
        Loaded += (_, _) =>
        {
            if (!_fl) return;
            _fl = false;
            SelectedItem = Nav.Items[0] as SelectionListItem;
        };
        PropertyChanged += (s, e) =>
        {
            if (SelectedItem == null || e.PropertyName != nameof(SelectedItem)) return;
            if (SelectedItem.Tag is not IAurelioPage page) return;
            page.RootElement.IsVisible = false;
            page.InAnimator.Animate();
        };
    }

    public MinecraftInstancePage()
    {
    }

    public RecordMinecraftEntry Entry { get; }

    public SelectionListItem SelectedItem
    {
        get => _selectedItem;
        set => SetField(ref _selectedItem, value);
    }

    public OverViewPage OverViewPage { get; }
    public ModPage ModPage { get; }
    public SavePage SavePage { get; }
    public ShaderPackPage ShaderPackPage { get; }
    public ScreenshotPage ScreenshotPage { get; }
    public ResourcePackPage ResourcePackPage { get; }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public void OnClose()
    {
        // 清理各个子页面的资源
        ScreenshotPage?.OnClose();

        // // 清理图片缓存以释放内存
        // ImageCache.ClearCache();
    }

    public void OpenFolder(MinecraftSpecialFolder folder)
    {
        App.TopLevel.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(
            Calculator.GetMinecraftSpecialFolder(Entry.MlEntry, folder)));
    }
}