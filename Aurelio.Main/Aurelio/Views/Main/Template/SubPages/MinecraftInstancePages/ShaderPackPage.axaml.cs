using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Enum.Minecraft;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using MinecraftLaunch.Base.Models.Game;

namespace Aurelio.Views.Main.Template.SubPages.MinecraftInstancePages;

public partial class ShaderPackPage : PageMixModelBase , IAurelioPage
{
    private readonly ObservableCollection<MinecraftLocalResourcePackEntry> _items = [];
    public ObservableCollection<MinecraftLocalResourcePackEntry> FilteredItems { get; set; } = [];
    private readonly MinecraftEntry _entry;
    private string _filter = string.Empty;

    public string Filter
    {
        get => _filter;
        set => SetField(ref _filter, value);
    }

    public ShaderPackPage(MinecraftEntry entry)
    {
        _entry = entry;
        InitializeComponent();
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        LoadItems();
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Filter))
            {
                FilterItems();
            }
        };
        Loaded += (_, _) => { LoadItems(); };
        DataContext = this;
        RefreshModBtn.Click += (_, _) => { LoadItems(); };
        DeselectAllModBtn.Click += (_, _) => { ModManageList.SelectedIndex = -1; };
        SelectAllModBtn.Click += (_, _) => { ModManageList.SelectAll(); };
        OpenFolderBtn.Click += (_, _) =>
        {
            var path = Public.Module.Value.Minecraft.Calculator.GetMinecraftSpecialFolder(_entry, MinecraftSpecialFolder.ShaderPacksFolder);
            Public.Module.IO.Local.Setter.TryCreateFolder(path);
            _ = OpenFolder(path);
        };
        DeleteSelectModBtn.Click += async (_, _) =>
        {
            var items = ModManageList.SelectedItems;
            if (items is null) return;
            var text = (from object? item in items select item as MinecraftLocalResourcePackEntry).Aggregate(string.Empty,
                (current, mod) => current + $"• {Path.GetFileName(mod.Name)}\n");

            var title = Data.DesktopType == DesktopType.Windows
                ? MainLang.MoveToRecycleBin
                : MainLang.DeleteSelect;
            var dialog = await ShowDialogAsync(title, text, b_cancel: MainLang.Cancel,
                b_primary: MainLang.Ok);
            if (dialog != ContentDialogResult.Primary) return;

            foreach (var item in items)
            {
                var file = item as MinecraftLocalResourcePackEntry;
                if (Data.DesktopType == DesktopType.Windows)
                {
                    try
                    {
                        FileSystem.DeleteFile(file.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
                else
                {
                    File.Delete(file.Path);
                }
            }

            LoadItems();
        };
        ModManageList.SelectionChanged += (_, _) =>
        {
            SelectedModCount.Text = $"{MainLang.SelectedItem} {ModManageList.SelectedItems.Count}";
        };
        SelectedModCount.Text = $"{MainLang.SelectedItem} 0";
    }

    private void LoadItems()
    {
        _items.Clear();

        var files = Directory.GetFiles(
            Public.Module.Value.Minecraft.Calculator.GetMinecraftSpecialFolder(_entry, MinecraftSpecialFolder.ShaderPacksFolder)
            , "*.*", System.IO.SearchOption.AllDirectories);
        foreach (var file in files)

            if (Path.GetExtension(file) == ".zip")
                _items.Add(new MinecraftLocalResourcePackEntry
                {
                    Name = Path.GetFileName(file)[..(Path.GetFileName(file).Length - 4)], Path = file,
                    Icon = null, Description = $"{MainLang.ImportTime}: {new FileInfo(file).CreationTime}"
                });

        FilterItems();
    }

    private void FilterItems()
    {
        FilteredItems.Clear();
        _items.Where(item => item.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            .ToList().OrderBy(mod => mod.Name).ToList().ForEach(mod => FilteredItems.Add(mod));
        NoMatchResultTip.IsVisible = FilteredItems.Count == 0;
        SelectedModCount.Text = $"{MainLang.SelectedItem} {ModManageList.SelectedItems.Count}";
    }
    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
}