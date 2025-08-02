global using static Aurelio.Public.Module.Ui.Overlay;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Aurelio.Plugin.Base;
using Aurelio.Plugin.Minecraft.Service.Minecraft;
using Aurelio.Plugin.Minecraft.Views;
using Aurelio.Plugin.Minecraft.Views.SettingPages;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Setting;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module;
using Aurelio.Public.Module.Plugin.Events;
using Aurelio.Public.Module.Ui;
using Aurelio.Views.Main.Pages;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using MinecraftLaunch;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json;
using Ursa.Controls;
using SettingTabPage = Aurelio.Plugin.Minecraft.Views.SettingTabPage;

namespace Aurelio.Plugin.Minecraft;

public partial class Main : IPlugin
{
    public string Id { get; set; } = "Yeppioo.Aurelio.Plugin.Minecraft";
    public string Name { get; set; } = "Minecraft Plugin";
    public string Author { get; set; } = "Yeppioo (yeppioo.vip)";
    public string Description { get; set; } = "Provides Minecraft support for Aurelio.";
    public object SettingPage
    {
        get => new SettingTabPage();
        set => Console.Write("");
    }

    public Version Version { get; set; } = Version.Parse("1.0.2");
    public RequirePluginEntry[] Require { get; set; } = [];

    public object? PackageInfo { get; set; } = new NugetPackage("Yeppioo.Aurelio.Plugin.Minecraft");

    public int Execute()
    {
        InitEvents.AfterUiLoaded += AppEventsOnAfterUiLoaded;
        InitEvents.BeforeUiLoaded += AppEventsOnBeforeUiLoaded;
        InitEvents.BeforeReadSettings += AppEventsOnBeforeReadSettings;
        InitEvents.MoreMenuLoaded += InitEventsOnMoreMenuLoaded;
        AppEvents.SaveSettings += AppEventsOnSaveSettings;
        return 0;
    }

    private void InitEventsOnMoreMenuLoaded(object? sender)
    {
        var menu = sender as MoreButtonMenu;
        var i = menu.MenuFlyout.Items.FirstOrDefault(x =>
            (string)((MenuItem)x).Header == MainLang.Tab) as MenuItem;
        i?.Items.Add(new MenuItem
        {
            Header = MainLang.MinecraftInstance,
            Command = new RelayCommand(() => { App.UiRoot.TogglePage(null, new MinecraftInstancesTabPage()); }),
            Icon = new PathIcon
            {
                Height = 16, Width = 17, Margin = new Thickness(0, 0, -1, 0),
                Data = Icons.Thumbtack
            }
        });
    }


    private void AppEventsOnSaveSettings(object? sender, EventArgs e)
    {
        File.WriteAllText(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Yeppioo.Aurelio",
            "Aurelio.Plugin.Minecraft.Setting.Yeppioo"), MinecraftPluginData.MinecraftPluginSettingEntry.AsJson());
    }

    private void AppEventsOnBeforeReadSettings(object? sender, EventArgs e)
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Yeppioo.Aurelio",
            "Aurelio.Plugin.Minecraft.Setting.Yeppioo");
        if (!File.Exists(path))
            File.WriteAllText(path, new MinecraftPluginSettingEntry().AsJson());
        MinecraftPluginData.MinecraftPluginSettingEntry =
            JsonConvert.DeserializeObject<MinecraftPluginSettingEntry>(File.ReadAllText(path));
        Service.UpdateSetting.Main();
        Service.AggregateSearch.Main();
    }

    private void AppEventsOnBeforeUiLoaded(object? sender, EventArgs e)
    {
        HttpUtil.Initialize();
        DownloadMirrorManager.MaxThread = 128;
        ServicePointManager.DefaultConnectionLimit = int.MaxValue;
    }


    private void AppEventsOnAfterUiLoaded(object? sender, EventArgs e)
    {
        MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftAccounts.CollectionChanged +=
            (_, _) => PublicEvents.OnUpdateAggregateSearchEntries();
        UiProperty.LaunchPages.Add(new LaunchPageEntry()
        {
            Id = "MinecraftInstances",
            Header = MainLang.MinecraftInstance,
            Page = new MinecraftInstancesTabPage()
        });
        _ = MinecraftInstancesHandler.Load(MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftFolderEntries
            .Select(x => x.Path).ToArray());
        InitNav();
    }

    private static void InitNav()
    {
        PageEvents.PageNavInit += sender =>
        {
            var nav = sender as SelectionList;
            nav.Items.Insert(0, new SelectionListItem
            {
                Tag = new AccountPage(),
                Content = new DockPanel
                {
                    Children =
                    {
                        new PathIcon
                        {
                            Data = StreamGeometry.Parse(
                                "F1 M 10 2.421875 C 4.822311 2.421875 0.625 6.619186 0.625 11.796875 C 0.625 16.974564 4.822311 21.171875 10 21.171875 C 15.177689 21.171875 19.375 16.974564 19.375 11.796875 C 19.375 6.619186 15.177689 2.421875 10 2.421875 Z M 10 6.796875 C 11.553345 6.796875 12.8125 8.056107 12.8125 9.609375 C 12.8125 11.162643 11.553345 12.421875 10 12.421875 C 8.446808 12.421875 7.1875 11.162643 7.1875 9.609375 C 7.1875 8.056107 8.446808 6.796875 10 6.796875 Z M 10 18.671875 C 7.922134 18.671875 6.060715 17.741547 4.799042 16.279907 C 5.533524 15.093842 6.835861 14.296875 8.333359 14.296875 L 11.666641 14.296875 C 13.164291 14.296875 14.466553 15.093765 15.200958 16.279831 C 13.939209 17.741547 12.077866 18.671875 10 18.671875 Z "),
                            Height = 15,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 0, 3, 0),
                            VerticalAlignment = VerticalAlignment.Center,
                            Width = 15
                        },
                        new TextBlock
                        {
                            Text = MainLang.Account,
                            Margin = new Thickness(5, 0, 0, 0)
                        }
                    }
                }
            });
            nav.Items.Insert(0, new SelectionListItem
            {
                Tag = new LaunchPage(),
                Content = new DockPanel
                {
                    Children =
                    {
                        new PathIcon
                        {
                            Data = StreamGeometry.Parse(
                                "F1 M 19.730377 2.548828 C 19.686432 2.338867 19.45694 2.109375 19.246979 2.06543 C 17.992172 1.796875 17.010765 1.796875 16.029396 1.796875 C 12.001305 1.796875 9.579582 3.955078 7.777901 6.796875 L 3.705864 6.796875 C 3.071136 6.796875 2.314339 7.260742 2.031136 7.832031 L 0.102539 11.689453 C 0.039062 11.821289 0.009766 11.962891 0 12.109375 C 0.004883 12.626953 0.419884 13.046875 0.942345 13.046875 L 4.999733 13.046875 C 7.069931 13.046875 8.749542 14.726562 8.749542 16.796875 L 8.749542 20.859375 C 8.749542 21.376953 9.169426 21.796875 9.687004 21.796875 C 9.83345 21.791992 9.975052 21.757812 10.106888 21.699219 L 13.964119 19.770508 C 14.530487 19.482422 14.999161 18.730469 14.999161 18.095703 L 14.999161 14.013672 C 17.831039 12.207031 19.998894 9.780273 19.998894 5.771484 C 20.003777 4.785156 20.003777 3.803711 19.730377 2.548828 Z M 14.999161 8.359375 C 14.139862 8.359375 13.436775 7.661133 13.436775 6.796875 C 13.441658 5.932617 14.139862 5.234375 15.004044 5.234375 C 15.86338 5.234375 16.561584 5.932617 16.561584 6.796875 C 16.561584 7.661133 15.86338 8.359375 14.999161 8.359375 Z M 1.391525 15.551758 C 0.385704 16.557617 -0.117188 19.086914 0.024414 21.772461 C 2.724457 21.914062 5.243874 21.40625 6.249657 20.400391 C 7.821846 18.828125 7.924385 16.733398 6.493797 15.302734 C 5.06321 13.876953 2.968597 13.974609 1.391525 15.551758 Z M 4.584732 18.833008 C 4.247818 19.165039 3.408012 19.335938 2.509651 19.287109 C 2.460823 18.393555 2.626801 17.548828 2.963715 17.216797 C 3.49102 16.689453 4.189224 16.655273 4.662857 17.133789 C 5.141335 17.607422 5.107155 18.305664 4.584732 18.833008 Z "),
                            Height = 13,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 0, 1, 0),
                            VerticalAlignment = VerticalAlignment.Center,
                            Width = 14
                        },
                        new TextBlock
                        {
                            Text = MainLang.Account,
                            Margin = new Thickness(5, 0, 0, 0)
                        }
                    }
                }
            });
        };
    }
}