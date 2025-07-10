using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Enum.Minecraft;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Avalonia.Media.Imaging;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using MinecraftLaunch.Base.Models.Game;
using Newtonsoft.Json.Linq;

namespace Aurelio.Views.Main.Template.SubPages.MinecraftInstancePages;

public partial class ResourcePackPage : PageMixModelBase, IAurelioPage
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

    public ResourcePackPage(MinecraftEntry entry)
    {
        _entry = entry;
        InitializeComponent();
        LoadItems();
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
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
            var path = Public.Module.Value.Minecraft.Calculator.GetMinecraftSpecialFolder(_entry,
                MinecraftSpecialFolder.ResourcePacksFolder);
            Public.Module.IO.Local.Setter.TryCreateFolder(path);
            _ = OpenFolder(path);
        };
        DeleteSelectModBtn.Click += async (_, _) =>
        {
            var items = ModManageList.SelectedItems;
            if (items is null) return;
            var text = (from object? item in items select item as MinecraftLocalResourcePackEntry).Aggregate(
                string.Empty,
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
            Public.Module.Value.Minecraft.Calculator.GetMinecraftSpecialFolder(_entry,
                MinecraftSpecialFolder.ResourcePacksFolder)
            , "*.*", System.IO.SearchOption.AllDirectories);
        foreach (var file in files)

            if (Path.GetExtension(file) == ".zip")
                _items.Add(new MinecraftLocalResourcePackEntry
                {
                    Name = Path.GetFileName(file)[..(Path.GetFileName(file).Length - 4)], Path = file,
                    Icon = GetIconFromZip(file), Description = GetDescriptionFromZip(file)
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


    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }

    public Bitmap GetIconFromZip(string zipFilePath)
    {
        if (!File.Exists(zipFilePath))
        {
            return null;
        }

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
        if (!File.Exists(zipFilePath))
        {
            return "string.Empty";
        }

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

    public ResourcePackPage()
    {
        InitializeComponent();
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
}