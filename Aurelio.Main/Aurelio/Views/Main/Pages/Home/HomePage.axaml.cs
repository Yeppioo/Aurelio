using Aurelio.Public.Classes.Entity;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Aurelio.Views.Main.Pages.Home;

public partial class HomePage : UserControl
{
    public HomePage()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        App.UiRoot.NavPages.FindById("Yep.Aurelio.ConvertTools").SubPages.Add(new NavPage("test", "test", new HomePage()));
    }
}