using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Aurelio.Public.Classes.Entity;
using Aurelio.ViewModels;
using SukiUI.Controls;

namespace Aurelio.Views.Main;

public partial class MainWindow : SukiWindow
{
    public MainViewModel ViewModel => DataContext as MainViewModel;
    public IEnumerable<NavPage> NavPages => ViewModel.NavPages;
    public IEnumerable<NavPage> FooterNavPages => ViewModel.FooterNavPages;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        Loaded += (_, _) => { Public.Module.App.Init.UiLoaded.Main(); };
    }
}