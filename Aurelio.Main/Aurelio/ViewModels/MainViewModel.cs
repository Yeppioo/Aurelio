using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui;
using Aurelio.Views.Main.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Material.Icons;

namespace Aurelio.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ObservableCollection<NavItemEntry> NavItems { get; set; } =
    [
        new(canClose: false, title: MainLang.MainPage, icon: Icons.FromMaterial(MaterialIconKind.Home),
            content: new MainPage()),
    ];

    public ObservableCollection<NewPageEntry> NewPageItems { get; set; } = [];

    [ObservableProperty] private NavItemEntry _selectedItem;

    public MainViewModel()
    {
        SelectedItem = NavItems[0];
        NewPageItems.AddRange(NewPageConfig.NewPageEntries.OrderBy(x=>x.Title));
    }
}