using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Service.Minecraft;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Aurelio.Views.Main.Template;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Aurelio.Views.Main.InstancePages;

public partial class HomeTabPage : PageMixModelBase, IAurelioTabPage
{
    private double _containerWidth;


    private string _searchText = string.Empty;

    public HomeTabPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
        PageInfo = new PageInfoEntry
        {
            CanClose = false,
            Title = MainLang.Launch,
            Icon = Icons.Home
        };
    }

    public new static Data Data => Data.Instance;

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetField(ref _searchText, value);
            HandleMinecraftInstances.Search(SearchText);
        }
    }

    public double ContainerWidth
    {
        get => _containerWidth;
        set => SetField(ref _containerWidth, value);
    }


    public TabEntry HostTab { get; set; }

    public PageInfoEntry PageInfo { get; }

    public void OnClose()
    {
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }

    private void BindingEvent()
    {
        SearchBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter) HandleMinecraftInstances.Search(SearchText);
        };
        SearchButton.Click += (_, _) => { HandleMinecraftInstances.Search(SearchText); };
        MinecraftCardsContainerRoot.SizeChanged += (_, _) =>
        {
            ContainerWidth = MinecraftCardsContainerRoot.Bounds.Width;
        };
    }

    private void MinecraftCardBorder_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (((Border)sender).Tag is not RecordMinecraftEntry entry) return;
        var tab = new TabEntry(new MinecraftInstancePage(entry));
        if (this.GetVisualRoot() is TabWindow window)
            window.CreateTab(tab);
        else
            App.UiRoot.CreateTab(tab);
    }

    private async void IconBorder_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var border = (Border)sender;
        var icon = (PathIcon)((Grid)border.Child).Children[0];
        (border.Tag as RecordMinecraftEntry)?.Launch(border);
        e.Handled = true;
        border.IsEnabled = false;
        icon.Data = Geometry.Parse(
            "F1 M 19.730377 2.548828 C 19.686432 2.338867 19.45694 2.109375 19.246979 2.06543 C 17.992172 1.796875 17.010765 1.796875 16.029396 1.796875 C 12.001305 1.796875 9.579582 3.955078 7.777901 6.796875 L 3.705864 6.796875 C 3.071136 6.796875 2.314339 7.260742 2.031136 7.832031 L 0.102539 11.689453 C 0.039062 11.821289 0.009766 11.962891 0 12.109375 C 0.004883 12.626953 0.419884 13.046875 0.942345 13.046875 L 4.999733 13.046875 C 7.069931 13.046875 8.749542 14.726562 8.749542 16.796875 L 8.749542 20.859375 C 8.749542 21.376953 9.169426 21.796875 9.687004 21.796875 C 9.83345 21.791992 9.975052 21.757812 10.106888 21.699219 L 13.964119 19.770508 C 14.530487 19.482422 14.999161 18.730469 14.999161 18.095703 L 14.999161 14.013672 C 17.831039 12.207031 19.998894 9.780273 19.998894 5.771484 C 20.003777 4.785156 20.003777 3.803711 19.730377 2.548828 Z M 14.999161 8.359375 C 14.139862 8.359375 13.436775 7.661133 13.436775 6.796875 C 13.441658 5.932617 14.139862 5.234375 15.004044 5.234375 C 15.86338 5.234375 16.561584 5.932617 16.561584 6.796875 C 16.561584 7.661133 15.86338 8.359375 14.999161 8.359375 Z M 1.391525 15.551758 C 0.385704 16.557617 -0.117188 19.086914 0.024414 21.772461 C 2.724457 21.914062 5.243874 21.40625 6.249657 20.400391 C 7.821846 18.828125 7.924385 16.733398 6.493797 15.302734 C 5.06321 13.876953 2.968597 13.974609 1.391525 15.551758 Z M 4.584732 18.833008 C 4.247818 19.165039 3.408012 19.335938 2.509651 19.287109 C 2.460823 18.393555 2.626801 17.548828 2.963715 17.216797 C 3.49102 16.689453 4.189224 16.655273 4.662857 17.133789 C 5.141335 17.607422 5.107155 18.305664 4.584732 18.833008 Z ");
        icon.Margin = new Thickness(0, 0, -2, 0);
        await Task.Delay(1200);
        border.IsEnabled = true;
        icon.Margin = new Thickness(0, 0, -5, 0);
        icon.Data = Geometry.Parse(
            "M73 39c-14.8-9.1-33.4-9.4-48.5-.9S0 62.6 0 80L0 432c0 17.4 9.4 33.4 24.5 41.9s33.7 8.1 48.5-.9L361 297c14.3-8.7 23-24.2 23-41s-8.7-32.2-23-41L73 39z");
    }
}