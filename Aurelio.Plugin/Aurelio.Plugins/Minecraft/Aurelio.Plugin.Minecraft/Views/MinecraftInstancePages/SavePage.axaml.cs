using System.Collections.ObjectModel;
using Aurelio.Plugin.Minecraft.Classes.Enum.Minecraft;
using Aurelio.Plugin.Minecraft.Classes.Minecraft;
using Aurelio.Plugin.Minecraft.Service.Minecraft;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Extensions;

namespace Aurelio.Plugin.Minecraft.Views.MinecraftInstancePages;

public partial class SavePage : PageMixModelBase, IAurelioPage
{
    private readonly MinecraftEntry _entry;
    private readonly ObservableCollection<MinecraftLocalSaveEntry> _items = [];
    private string _filter = string.Empty;

    public SavePage(MinecraftEntry entry)
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
            var path = Calculator.GetMinecraftSpecialFolder(_entry,
                MinecraftSpecialFolder.SavesFolder);
            Setter.TryCreateFolder(path);
            _ = OpenFolder(path);
        };
        ShortInfo = $"{_entry.Id} / {MainLang.Saves}";
        DeleteSelectModBtn.Click += async (sender, _) =>
        {
            var items = ModManageList.SelectedItems;
            if (items is null) return;
            var text = Enumerable.Aggregate<MinecraftLocalSaveEntry, string>((from object? item in items select item as MinecraftLocalSaveEntry), string.Empty,
                (current, mod) => current + $"• {Path.GetFileName((string?)mod.Name)}\n");

            var title = Public.Const.Data.DesktopType == DesktopType.Windows
                ? MainLang.MoveToRecycleBin
                : MainLang.DeleteSelect;
            var dialog = await ShowDialogAsync(title, text, b_cancel: MainLang.Cancel,
                b_primary: MainLang.Ok, sender: sender as Control);
            if (dialog != ContentDialogResult.Primary) return;

            foreach (var item in items)
            {
                var file = item as MinecraftLocalSaveEntry;
                if (Public.Const.Data.DesktopType == DesktopType.Windows)
                    FileSystem.DeleteDirectory(file.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                else
                    Directory.Delete(file.Path);
            }

            LoadItems();
        };
        ModManageList.SelectionChanged += (_, _) =>
        {
            SelectedModCount.Text = $"{MainLang.SelectedItem} {ModManageList.SelectedItems.Count}";
        };
        SelectedModCount.Text = $"{MainLang.SelectedItem} 0";

        // Add event handlers for the new buttons
        ModManageList.Loaded += (_, _) =>
        {
            // Find all buttons and attach event handlers
            AttachButtonEventHandlers();
        };
    }
    
    private string _shortInfo = string.Empty;

    public string ShortInfo
    {
        get => _shortInfo;
        set => SetField(ref _shortInfo, value);
    }

    public SavePage()
    {
    }

    public ObservableCollection<MinecraftLocalSaveEntry> FilteredItems { get; set; } = [];

    public string Filter
    {
        get => _filter;
        set => SetField(ref _filter, value);
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }

    private async void LoadItems()
    {
        _items.Clear();

        var saves = await GetSaves(_entry);
        saves.ForEach(save =>
        {
            _items.Add(new MinecraftLocalSaveEntry
            {
                Name = save.FolderName, Path = Path.Combine(Calculator.GetMinecraftSpecialFolder
                    (_entry, MinecraftSpecialFolder.SavesFolder), save.FolderName),
                Icon = save.IconBitmap, Callback = LoadItems, SaveInfo = save,
                Description =
                    $"{MainLang.CreateTime}: {save.CreationTime}, {MainLang.LastModifiedTime}: {save.LastWriteTime}"
            });
        });
        ShortInfo = $"{_entry.Id} / {MainLang.Saves} / 已加载 {_items.Count} 个存档";

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

    public static async Task<List<SaveInfo>> GetSaves(MinecraftEntry entry)
    {
        var folderInfos = new List<SaveInfo>();
        var parentPath = Calculator.GetMinecraftSpecialFolder(entry, MinecraftSpecialFolder.SavesFolder);
        var folders = Directory.GetDirectories(parentPath);
        foreach (var folderPath in folders)
            try
            {
                var folderName = Path.GetFileName(folderPath);
                if (!File.Exists(Path.Combine(folderPath, "level.dat"))) continue;
                var creationTime = Directory.GetCreationTime(folderPath);
                var lastWriteTime = Directory.GetLastWriteTime(folderPath);
                Bitmap iconBitmap = null;
                SaveEntry? saveNBTEntry = null;
                string iconPath = null;
                try
                {
                    saveNBTEntry = await entry.GetNBTParser().ParseSaveAsync(folderName);
                    iconPath = saveNBTEntry.IconFilePath;
                }
                catch
                {
                }

                if (File.Exists(iconPath ?? Path.Combine(folderPath, "icon.png")))
                    try
                    {
                        using var stream =
                            new MemoryStream(
                                await File.ReadAllBytesAsync(iconPath ?? Path.Combine(folderPath, "icon.png")));
                        iconBitmap = new Bitmap(stream);
                    }
                    catch
                    {
                        iconBitmap = null;
                    }

                var datFileCount = 0;
                var playerDataPath = Path.Combine(folderPath, "playerdata");
                if (Directory.Exists(playerDataPath))
                    try
                    {
                        datFileCount = Directory.GetFiles(playerDataPath, "*.dat").Length;
                    }
                    catch
                    {
                        datFileCount = 0;
                    }

                var zipFileCount = 0;
                var datapacksPath = Path.Combine(folderPath, "datapacks");
                if (Directory.Exists(datapacksPath))
                    try
                    {
                        zipFileCount = Directory.GetFiles(datapacksPath, "*.zip").Length;
                    }
                    catch
                    {
                        zipFileCount = 0;
                    }

                if (saveNBTEntry == null)
                    saveNBTEntry = new SaveEntry
                    {
                        LastPlayed = new DateTime(1970, 1, 1, 0, 0, 0),
                        Version = "Unknown",
                        AllowCommands = false,
                        GameType = -1
                    };

                folderInfos.Add(new SaveInfo
                {
                    FolderName = folderName,
                    CreationTime = creationTime,
                    LastWriteTime = lastWriteTime,
                    LastPlayTime = saveNBTEntry.LastPlayed,
                    Version = saveNBTEntry.Version,
                    Seed = saveNBTEntry.Seed,
                    AllowCommands = saveNBTEntry.AllowCommands,
                    GameType = saveNBTEntry.GameType,
                    IconBitmap = iconBitmap,
                    DatFileCount = datFileCount,
                    ZipFileCount = zipFileCount,
                    FolderPath = folderPath
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

        return folderInfos;
    }

    private void AttachButtonEventHandlers()
    {
        // Find all buttons in the ListBox and attach event handlers
        var listBoxItems = ModManageList.GetLogicalDescendants().OfType<ListBoxItem>();
        foreach (var listBoxItem in listBoxItems)
        {
            var openFolderBtn = listBoxItem.FindNameScope()?.Find("OpenFolderBtn") as Button;
            var showInfoBtn = listBoxItem.FindNameScope()?.Find("ShowInfoBtn") as Button;
            var deleteBtn = listBoxItem.FindNameScope()?.Find("DeleteSaveBtn") as Button;

            if (openFolderBtn != null)
            {
                openFolderBtn.Click -= OnOpenFolderClick; // Remove existing handler
                openFolderBtn.Click += OnOpenFolderClick;
            }

            if (showInfoBtn != null)
            {
                showInfoBtn.Click -= OnShowInfoClick; // Remove existing handler
                showInfoBtn.Click += OnShowInfoClick;
            }

            if (deleteBtn != null)
            {
                deleteBtn.Click -= OnDeleteSaveClick; // Remove existing handler
                deleteBtn.Click += OnDeleteSaveClick;
            }
        }
    }

    private void OnOpenFolderClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is MinecraftLocalSaveEntry save)
        {
            save.OpenFolder();
        }
    }

    private async void OnShowInfoClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is MinecraftLocalSaveEntry save)
        {
            await save.ShowInfo(button);
        }
    }

    private async void OnDeleteSaveClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is MinecraftLocalSaveEntry save)
        {
            await save.Delete(button);
        }
    }
}