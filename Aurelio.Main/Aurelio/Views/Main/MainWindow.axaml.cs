using System.Collections.Generic;
using System.Collections.ObjectModel;
using Aurelio.Public.Classes.Entity;
using Aurelio.ViewModels;
using SukiUI.Controls;

namespace Aurelio.Views.Main;

public partial class MainWindow : SukiWindow
{
    public MainViewModel ViewModel => DataContext as MainViewModel;
    public IEnumerable<NavPage> NavPages => ViewModel.NavPages;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}