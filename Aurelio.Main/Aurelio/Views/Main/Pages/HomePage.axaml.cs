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
using Aurelio.ViewModels;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Material.Icons;

namespace Aurelio.Views.Main.Pages;

public partial class HomePage : PageMixModelBase, IAurelioPage
{
    public HomePage()
    {
        InitializeComponent();
        DataContext = this;
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
}