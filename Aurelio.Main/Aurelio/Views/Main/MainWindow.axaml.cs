using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Page;
using Aurelio.Public.Enum;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO.Local;
using Aurelio.ViewModels;
using Aurelio.Views.Main.Pages;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using HotAvalonia;
using Material.Icons;
using Ursa.Controls;

namespace Aurelio.Views.Main;

public partial class MainWindow : UrsaWindow
{
    public Button MenuButton;
    public MainViewModel ViewModel { get; set; } = new();
    public ObservableCollection<TabEntry> Tabs => ViewModel.Tabs;
    public TabEntry? SelectedTab => ViewModel.SelectedTab;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
        NewTabButton.DataContext = ViewModel;
        BindEvents();
#if RELEASE
        InitTitleBar();
#endif
    }

    [AvaloniaHotReload]
    private void InitTitleBar()
    {
        var settingButton = new Button()
        {
            Classes = { "title-bar-button", "big-title-bar-icon" },
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Content = Public.Module.Ui.Icon.FromMaterial(MaterialIconKind.Settings)
        };
        TitleBar.AddButton(settingButton);
        MenuButton = new Button()
        {
            Classes = { "title-bar-button-more" },
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Content = Public.Module.Ui.Icon.FromMaterial(MaterialIconKind.MoreVert)
        };
        TitleBar.AddButton(MenuButton);
        var c = new MoreButtonMenu();
        var menu = (MenuFlyout)c.MainControl!.Flyout;
        MenuButton.Flyout = menu;
        MenuButton.DataContext = new MoreButtonMenuCommands();
        settingButton.Click += (_, _) =>
        {
            var tab = Tabs.FirstOrDefault(x => x.Tag == "settsing");
            if (tab is null)
            {
                var newTab = new TabEntry(MainLang.Setting, PageInstance.SettingPage,
                    Public.Module.Ui.Icon.FromMaterial(MaterialIconKind.Settings))
                {
                    Tag = "setting"
                };
                Tabs.Add(newTab);
                ViewModel.SelectedTab = newTab;
            }
            else
            {
                if (SelectedTab == tab) return;
                ViewModel.SelectedTab = tab;
            }
        };
    }

    private void BindEvents()
    {
        NavScrollViewer.ScrollChanged += (_, _) => { ViewModel.IsTabMaskVisible = NavScrollViewer.Offset.X > 0; };
    }

    private void TabItem_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed) return;
        var c = (TabEntry)((Border)sender).Tag;
        c.Close();
    }
}