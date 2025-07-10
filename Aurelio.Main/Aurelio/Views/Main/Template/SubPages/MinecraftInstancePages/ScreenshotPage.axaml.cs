using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Aurelio.Public.Classes.Enum.Minecraft;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Controls;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.Module.Value.Minecraft;
using Aurelio.ViewModels;
using MinecraftLaunch.Base.Models.Game;

namespace Aurelio.Views.Main.Template.SubPages.MinecraftInstancePages;

public partial class ScreenshotPage : PageMixModelBase, IAurelioPage
{
    private readonly MinecraftEntry _entry;
    private readonly ObservableCollection<MinecraftLocalResourcePackEntry> _items = [];
    private string _filter = string.Empty;

    public ScreenshotPage(MinecraftEntry entry)
    {
        _entry = entry;
        InitializeComponent();
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        LoadItems();
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Filter)) FilterItems();
        };
        Loaded += (_, _) =>
        {
            LoadItems();
            // Viewer.TranslateY = 0;
            // Viewer.TranslateX = 0;
            // Viewer.Scale = 0.6;
        };
        DataContext = this;
        RefreshModBtn.Click += (_, _) => { LoadItems(); };
        OpenFolderBtn.Click += (_, _) =>
        {
            var path = Calculator.GetMinecraftSpecialFolder(_entry,
                MinecraftSpecialFolder.ScreenshotsFolder);
            Setter.TryCreateFolder(path);
            _ = OpenFolder(path);
        };
    }

    public string Filter
    {
        get => _filter;
        set => SetField(ref _filter, value);
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }

    private void LoadItems()
    {
        _items.Clear();

        var files = Directory.GetFiles(
            Calculator.GetMinecraftSpecialFolder(_entry,
                MinecraftSpecialFolder.ScreenshotsFolder)
            , "*.png", SearchOption.AllDirectories);
        foreach (var file in files)
            _items.Add(new MinecraftLocalResourcePackEntry
            {
                Name = Path.GetFileName(file), Path = file,
                Icon = null, Description = $"{MainLang.ImportTime}: {new FileInfo(file).CreationTime}"
            });

        FilterItems();
    }

    private void FilterItems()
    {
        Container.Children.Clear();
        _items.Where(item => item.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            .ToList().OrderBy(mod => mod.Name).ToList().ForEach(mod => Container.Children.Add(
                new ScreenshotEntry(mod.Name, mod.Path, LoadItems)));
        NoMatchResultTip.IsVisible = Container.Children.Count == 0;
    }
}