using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Enum.Minecraft;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Services.Minecraft;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.Module.Value;
using Aurelio.ViewModels;
using Avalonia.Media.Imaging;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Extensions;
using Calculator = Aurelio.Public.Module.Services.Minecraft.Calculator;

namespace Aurelio.Views.Main.Template.SubPages.MinecraftInstancePages;

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
        DeleteSelectModBtn.Click += async (_, _) =>
        {
            var items = ModManageList.SelectedItems;
            if (items is null) return;
            var text = (from object? item in items select item as MinecraftLocalSaveEntry).Aggregate(string.Empty,
                (current, mod) => current + $"• {Path.GetFileName(mod.Name)}\n");

            var title = Data.DesktopType == DesktopType.Windows
                ? MainLang.MoveToRecycleBin
                : MainLang.DeleteSelect;
            var dialog = await ShowDialogAsync(title, text, b_cancel: MainLang.Cancel,
                b_primary: MainLang.Ok);
            if (dialog != ContentDialogResult.Primary) return;

            foreach (var item in items)
            {
                var file = item as MinecraftLocalSaveEntry;
                if (Data.DesktopType == DesktopType.Windows)
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
                Console.WriteLine($"Error processing folder {folderPath}: {ex.Message}");
            }

        return folderInfos;
    }
}