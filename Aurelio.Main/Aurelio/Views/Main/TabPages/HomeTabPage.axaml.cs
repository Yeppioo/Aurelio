using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Aurelio.Views.Main.Template;
using Avalonia.Input;

namespace Aurelio.Views.Main.TabPages;

public partial class HomeTabPage : PageMixModelBase, IAurelioTabPage
{
    public static Data Data => Data.Instance;

    public HomeTabPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
    }

    private void BindingEvent()
    {
        SearchBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter) Public.Module.Service.MinecraftInstances.Search(SearchText);
        };
        SearchButton.Click += (_, _) => { Public.Module.Service.MinecraftInstances.Search(SearchText); };
        MinecraftCardsContainerRoot.SizeChanged += (_, _) =>
        {
            ContainerWidth = MinecraftCardsContainerRoot.Bounds.Width;
        };
    }


    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetField(ref _searchText, value);
            Public.Module.Service.MinecraftInstances.Search(SearchText);
        }
    }

    

    public TabEntry HostTab { get; set; }

    public PageInfoEntry PageInfo { get; } = new()
    {
        CanClose = false,
        Title = MainLang.Launch,
        Icon = Icons.Home
    };

    public void OnClose()
    {
    }

    public Border RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }

    public double ContainerWidth
    {
        get => _containerWidth;
        set => SetField(ref _containerWidth, value);
    }

    private double _containerWidth;

    private void IconBorder_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (((Border)sender).Tag is not RecordMinecraftEntry entry) return;
        var tab = new TabEntry(new MinecraftInstancePage(entry));
        App.UiRoot.CreateTab(tab);
    }
}