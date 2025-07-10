using System.IO;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum.Minecraft;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Aurelio.Views.Main.Template.MinecraftInstancePages;
using Avalonia.Platform.Storage;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Ursa.Controls;

namespace Aurelio.Views.Main.Template;

public partial class MinecraftInstancePage : PageMixModelBase, IAurelioTabPage
{
    public RecordMinecraftEntry Entry { get; }
    private SelectionListItem _selectedItem;

    public SelectionListItem SelectedItem
    {
        get => _selectedItem;
        set => SetField(ref _selectedItem, value);
    }

    private bool _fl = true;
    public OverViewPage OverViewPage { get; } 
    public ModPage ModPage { get; } 

    public MinecraftInstancePage(RecordMinecraftEntry entry)
    {
        Entry = entry;
        OverViewPage = new OverViewPage(Entry);
        ModPage = new ModPage(Entry.MlEntry);
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
            if(!_fl) return;
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

    public void OpenFolder(MinecraftSpecialFolder folder)
    {
        App.TopLevel.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(
            Public.Module.Value.Minecraft.Calculator.GetMinecraftSpecialFolder(Entry.MlEntry, folder)));
    }

    public Border RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public void OnClose()
    {
    }
}