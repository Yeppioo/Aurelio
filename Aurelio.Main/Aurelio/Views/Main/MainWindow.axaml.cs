using Aurelio.ViewModels;
using Ursa.Controls;

namespace Aurelio.Views.Main;

public partial class MainWindow : UrsaWindow
{
    public MainViewModel ViewModel { get; set; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
        BindEvents();
    }

    private void BindEvents()
    {
    }
}