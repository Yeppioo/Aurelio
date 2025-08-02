using System.Collections.ObjectModel;
using Aurelio.Plugin.Minecraft.Classes.Enum.Minecraft;
using Aurelio.Plugin.Minecraft.Classes.Minecraft;
using Aurelio.Plugin.Minecraft.Service.Minecraft;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using MinecraftLaunch.Base.Models.Game;
using SearchOption = System.IO.SearchOption;

namespace Aurelio.Plugin.Minecraft.Views.MinecraftInstancePages;

public partial class ShaderPackPage : PageMixModelBase, IAurelioPage
{
    private readonly MinecraftEntry _entry;
    private readonly ObservableCollection<MinecraftLocalResourcePackEntry> _items = [];
    private string _filter = string.Empty;

    public ShaderPackPage(MinecraftEntry entry)
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
        Loaded += (_, _) => { LoadItems(); };
        DataContext = this;
        RefreshModBtn.Click += (_, _) => { LoadItems(); };
        DeselectAllModBtn.Click += (_, _) => { ModManageList.SelectedIndex = -1; };
        SelectAllModBtn.Click += (_, _) => { ModManageList.SelectAll(); };
        OpenFolderBtn.Click += (_, _) =>
        {
            var path = Calculator.GetMinecraftSpecialFolder(_entry, MinecraftSpecialFolder.ShaderPacksFolder);
            Setter.TryCreateFolder(path);
            _ = OpenFolder(path);
        };
        ShortInfo = $"{_entry.Id} / {MainLang.ShaderPack}";
        DeleteSelectModBtn.Click += async (sender, _) =>
        {
            var items = ModManageList.SelectedItems;
            if (items is null) return;
            var text = Enumerable.Aggregate<MinecraftLocalResourcePackEntry, string>((from object? item in items select item as MinecraftLocalResourcePackEntry), string.Empty,
                (current, mod) => current + $"• {Path.GetFileName((string?)mod.Name)}\n");

            var title = Public.Const.Data.DesktopType == DesktopType.Windows
                ? MainLang.MoveToRecycleBin
                : MainLang.DeleteSelect;
            var dialog = await ShowDialogAsync(title, text, b_cancel: MainLang.Cancel,
                b_primary: MainLang.Ok, sender: sender as Control);
            if (dialog != ContentDialogResult.Primary) return;

            foreach (var item in items)
            {
                var file = item as MinecraftLocalResourcePackEntry;
                if (Public.Const.Data.DesktopType == DesktopType.Windows)
                    try
                    {
                        FileSystem.DeleteFile(file.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                else
                    File.Delete(file.Path);
            }

            LoadItems();
        };
        ModManageList.SelectionChanged += (_, _) =>
        {
            SelectedModCount.Text = $"{MainLang.SelectedItem} {ModManageList.SelectedItems.Count}";
        };
        SelectedModCount.Text = $"{MainLang.SelectedItem} 0";
    }
    private string _shortInfo = string.Empty;

    public string ShortInfo
    {
        get => _shortInfo;
        set => SetField(ref _shortInfo, value);
    }
    public ShaderPackPage()
    {
    }

    public ObservableCollection<MinecraftLocalResourcePackEntry> FilteredItems { get; set; } = [];

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
            Calculator.GetMinecraftSpecialFolder(_entry, MinecraftSpecialFolder.ShaderPacksFolder)
            , "*.*", SearchOption.AllDirectories);
        foreach (var file in files)

            if (Path.GetExtension(file) == ".zip")
                _items.Add(new MinecraftLocalResourcePackEntry
                {
                    Name = Path.GetFileName(file)[..(Path.GetFileName(file).Length - 4)], Path = file,
                    Icon = null, Description = $"{MainLang.ImportTime}: {new FileInfo(file).CreationTime}"
                });
        ShortInfo = $"{_entry.Id} / {MainLang.ShaderPack} / 已加载 {_items.Count} 个光影包";
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
}