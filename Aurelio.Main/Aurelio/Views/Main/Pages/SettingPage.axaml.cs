using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Page;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aurelio.Views.Main.Pages;

public partial class SettingPage : PageMixModelBase, IAurelioPage
{
    public SettingPage()
    {
        InitializeComponent();
    }

    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }
    public void OnClose()
    {
    }
}