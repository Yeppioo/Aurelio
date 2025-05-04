using System.Collections.ObjectModel;
using Aurelio.Plugin.Base;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aurelio.Views.Main.Pages.Plugin;

public partial class PluginPage : UserControl
{
    public ObservableCollection<IPlugin> Plugins { get; } = [];
    public PluginPage()
    {
        InitializeComponent();
        DataContext = this;
    }
}