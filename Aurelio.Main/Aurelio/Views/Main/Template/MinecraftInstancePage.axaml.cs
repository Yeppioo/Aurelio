using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aurelio.Views.Main.Template;

public partial class MinecraftInstancePage : PageMixModelBase, IAurelioTabPage
{
    public RecordMinecraftEntry Entry { get; }

    public MinecraftInstancePage(RecordMinecraftEntry entry)
    {
        Entry = entry;
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        PageInfo = new PageInfoEntry
        {
            Title = Entry.Id,
            Icon = Icons.Seedling
        };
    }

    public MinecraftInstancePage()
    {
        InitializeComponent();
    }

    public Border RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public void OnClose()
    {
    }
}