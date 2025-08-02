using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Compression;
using System.Runtime.CompilerServices;
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
using Avalonia.Media.Imaging;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using MinecraftLaunch.Base.Models.Game;
using Newtonsoft.Json.Linq;
using SearchOption = System.IO.SearchOption;

namespace Aurelio.Plugin.Minecraft.Views.MinecraftInstancePages;

public partial class ResourcePackPage : PageMixModelBase, IAurelioPage
{
    private readonly MinecraftEntry _entry;
    private readonly ObservableCollection<MinecraftLocalResourcePackEntry> _items = [];
    private string _filter = string.Empty;

    public ResourcePackPage(MinecraftEntry entry)
    {
        _entry = entry;
        InitializeComponent();
        LoadItems();
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
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
                MinecraftSpecialFolder.ResourcePacksFolder);
            Setter.TryCreateFolder(path);
            _ = OpenFolder(path);
        };
        ShortInfo = $"{_entry.Id} / {MainLang.ResourcePacks}";

        DeleteSelectModBtn.Click += async (sender, _) =>
        {
            var items = ModManageList.SelectedItems;
            if (items is null) return;
            var text = (from object? item in items select item as MinecraftLocalResourcePackEntry).Aggregate<MinecraftLocalResourcePackEntry, string>(string.Empty,
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

    public Control BottomElement { get; set; }

    public ResourcePackPage()
    {
        InitializeComponent();
    }

    public ObservableCollection<MinecraftLocalResourcePackEntry> FilteredItems { get; set; } = [];

    public string Filter
    {
        get => _filter;
        set => SetField(ref _filter, value);
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }


    public new event PropertyChangedEventHandler? PropertyChanged;

    private void LoadItems()
    {
        _items.Clear();

        var files = Directory.GetFiles(
            Calculator.GetMinecraftSpecialFolder(_entry,
                MinecraftSpecialFolder.ResourcePacksFolder)
            , "*.*", SearchOption.AllDirectories);
        foreach (var file in files)

            if (Path.GetExtension(file) == ".zip")
                _items.Add(new MinecraftLocalResourcePackEntry
                {
                    Name = Path.GetFileName(file)[..(Path.GetFileName(file).Length - 4)], Path = file,
                    Icon = GetIconFromZip(file), Description = GetDescriptionFromZip(file)
                });

        ShortInfo = $"{_entry.Id} / {MainLang.ResourcePacks} / 已加载 {_items.Count} 个资源包";
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

    private new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private new void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }

    public Bitmap GetIconFromZip(string zipFilePath)
    {
        if (!File.Exists(zipFilePath)) return null;

        using var archive = ZipFile.OpenRead(zipFilePath);
        foreach (var entry in archive.Entries)
        {
            if (entry.FullName != "pack.png") continue;

            try
            {
                using var entryStream = entry.Open();
                using var memoryStream = new MemoryStream();
                entryStream.CopyTo(memoryStream);
                memoryStream.Position = 0;
                return Bitmap.DecodeToHeight(memoryStream, 28);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    public string GetDescriptionFromZip(string zipFilePath)
    {
        if (!File.Exists(zipFilePath)) return "string.Empty";

        using var archive = ZipFile.OpenRead(zipFilePath);
        foreach (var entry in archive.Entries)
        {
            if (entry.FullName != "pack.mcmeta") continue;
            using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream);
            var jsonContent = reader.ReadToEnd();
            var packData = JObject.Parse(jsonContent);
            var descriptionToken = packData["pack"]?["description"];
            return descriptionToken != null ? descriptionToken.ToString() : "string.Empty";
        }

        return "string.Empty";
    }
}