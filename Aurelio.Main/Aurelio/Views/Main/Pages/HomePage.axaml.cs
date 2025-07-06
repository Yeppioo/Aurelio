using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Page;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Material.Icons;

namespace Aurelio.Views.Main.Pages;

public partial class HomeTabPage : PageMixModelBase, IAurelioTabPage
{
    public HomeTabPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        BindingEvent();
    }

    private void BindingEvent()
    {
    }

    public TabEntry HostTab { get; set; }

    public PageInfoEntry PageInfo { get; } = new();

    public void OnClose()
    {
    }

    public Border RootElement { get; set; }
}