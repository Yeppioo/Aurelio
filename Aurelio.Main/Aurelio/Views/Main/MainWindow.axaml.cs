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
    public ObservableCollection<NavPage> NavPages => ViewModel.NavPages;
    public ObservableCollection<NavPage> FooterNavPages => ViewModel.FooterNavPages;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        NavMenu.SelectedItem = NavPages[0];
        Loaded += (_, _) => { Public.Module.App.Init.UiLoaded.Main(); };
    }
}