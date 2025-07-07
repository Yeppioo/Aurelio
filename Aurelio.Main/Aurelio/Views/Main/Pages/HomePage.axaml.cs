using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Aurelio.Views.Main.Template;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Material.Icons;

namespace Aurelio.Views.Main.Pages;

public partial class HomeTabPage : PageMixModelBase, IAurelioTabPage
{
    public static Data Data => Data.Instance;

    public HomeTabPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
    }

    private void BindingEvent()
    {
        SearchBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter) Search();
        };
        SearchButton.Click += (_, _) => { Search(); };
        MinecraftCardsContainerRoot.SizeChanged += (_, _) =>
        {
            ContainerWidth = MinecraftCardsContainerRoot.Bounds.Width;
        };
    }


    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set => SetField(ref _searchText, value);
    }

    public void Search(bool ui = true)
    {
        var filtered = Data.MinecraftInstances.First(x => x.Tag == "filtered");
        if (ui && SearchText.IsNullOrWhiteSpace())
        {
            filtered.Visible = false;
            Data.MinecraftInstances[1].Expanded = true;
            return;
        }

        filtered.Minecrafts.Clear();
        var all = Data.MinecraftInstances.First(x => x.Tag == "all").Minecrafts;
        var filteredItems = all.Where(item =>
            item.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
            item.ShortDescription.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        filtered.Minecrafts.AddRange(filteredItems);
        if (!ui) return;
        filtered.Visible = true;
        foreach (var minecraftCategoryEntry in Data.MinecraftInstances)
        {
            minecraftCategoryEntry.Expanded = false;
        }

        filtered.Expanded = true;
    }

    public TabEntry HostTab { get; set; }

    public PageInfoEntry PageInfo { get; } = new()
    {
        CanClose = false,
        Title = MainLang.Launch,
        Icon = Icons.Home
    };

    public void OnClose()
    {
    }

    public Border RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }

    public double ContainerWidth
    {
        get => _containerWidth;
        set => SetField(ref _containerWidth, value);
    }

    private double _containerWidth;

    private void IconBorder_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (((Border)sender).Tag is not RecordMinecraftEntry entry) return;
        var tab = new TabEntry(new MinecraftInstancePage(entry));
        App.UiRoot.CreateTab(tab);
    }
}