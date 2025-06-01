using Aurelio.Public.Const;
using Avalonia.Controls;

namespace Aurelio.Views.Main.Pages;

public partial class MainPage : UserControl
{
    public MainPage()
    {
        InitializeComponent();
        DataContext = Data.Instance;
        BindEvents();
    }

    private void BindEvents()
    {
    }
}