using System;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Types;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui;
using Avalonia.Controls;
using Avalonia.Media;
using Material.Icons;

namespace Aurelio.Views.Main.Pages;

public partial class MainPage : UserControl, IFunctionPage
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

    public void Dispose()
    {
    }

    public (string title, StreamGeometry icon) GetPageInfo()
    {
        return (MainLang.MainPage, Icon.FromMaterial(MaterialIconKind.Home));
    }

    public TabEntry HostTab { get; set; }
    public UserControl HostContent { get; set; }
}