using System.Collections.ObjectModel;
using Aurelio.Public.Classes.Entity;
using Aurelio.Views.Main;

namespace Aurelio.Plugin.Api;

public class Instance
{
    public static MainWindow MainWindow => App.UiRoot;
    public static ObservableCollection<NavPage> NavPages => App.UiRoot.NavPages;
    public static ObservableCollection<NavPage> FooterNavPages => App.UiRoot.FooterNavPages;
}