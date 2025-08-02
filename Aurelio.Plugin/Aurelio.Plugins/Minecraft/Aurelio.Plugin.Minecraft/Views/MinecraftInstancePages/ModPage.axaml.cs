using System.Collections.ObjectModel;
using System.IO.Compression;
using Aurelio.Plugin.Minecraft.Classes.Enum.Minecraft;
using Aurelio.Plugin.Minecraft.Classes.Minecraft;
using Aurelio.Plugin.Minecraft.Service.Minecraft;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.LogicalTree;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using MinecraftLaunch.Base.Models.Game;
using Newtonsoft.Json.Linq;
using Tomlyn;
using Tomlyn.Model;
using SearchOption = System.IO.SearchOption;

namespace Aurelio.Plugin.Minecraft.Views.MinecraftInstancePages;

public partial class ModPage : PageMixModelBase, IAurelioPage
{
    private readonly MinecraftEntry _entry;
    private readonly ObservableCollection<MinecraftLocalModEntry> _mods = [];
    private string _filter = string.Empty;
    private bool _isLoading;

    private string _shortInfo = string.Empty;

    public string ShortInfo
    {
        get => _shortInfo;
        set => SetField(ref _shortInfo, value);
    }
    
    public ModPage(MinecraftEntry entry)
    {
        _entry = entry;
        InitializeComponent();
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        LoadMods();
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Filter)) FilterMods();
        };
        DataContext = this;
        RefreshModBtn.Click += (_, _) => { LoadMods(); };
        // Loaded += (_, _) => { LoadMods(); };
        DeselectAllModBtn.Click += (_, _) => { ModManageList.SelectedIndex = -1; };
        SelectAllModBtn.Click += (_, _) => { ModManageList.SelectAll(); };
        DisableSelectModBtn.Click += (_, _) =>
        {
            var mods = ModManageList.SelectedItems;
            foreach (var item in mods)
            {
                var mod = item as MinecraftLocalModEntry;
                if (mod.FileName.Length <= 0) continue;
                if (Path.GetExtension(mod.Path) == ".jar")
                    File.Move(mod.Path, mod.Path + ".disabled");
            }

            LoadMods();
        };
        ShortInfo = $"{_entry.Id} / {MainLang.Mod}";
        EnableSelectModBtn.Click += (_, _) =>
        {
            var mods = ModManageList.SelectedItems;
            foreach (var item in mods)
            {
                var mod = item as MinecraftLocalModEntry;
                if (mod.FileName.Length <= 0) continue;
                if (string.IsNullOrWhiteSpace(Path.GetDirectoryName(mod.Path))) continue;
                if (Path.GetExtension(mod.Path) == ".disabled")
                    File.Move(mod.Path, Path.Combine(Path.GetDirectoryName(mod.Path)!, $"{mod.FileName}.jar"));
            }

            LoadMods();
        };
        DeleteSelectModBtn.Click += async (sender, _) =>
        {
            var mods = ModManageList.SelectedItems;
            if (mods is null || mods.Count <= 0) return;
            var text = Enumerable.Aggregate<MinecraftLocalModEntry, string>((from object? item in mods select item as MinecraftLocalModEntry), string.Empty,
                (current, mod) => current + $"• {Path.GetFileName((string?)mod.FileName)}\n");

            var title = Data.DesktopType == DesktopType.Windows
                ? MainLang.MoveToRecycleBin
                : MainLang.DeleteSelect;
            var dialog = await Public.Module.Ui.Overlay.ShowDialogAsync(title, text, b_cancel: MainLang.Cancel,
                b_primary: MainLang.Ok, sender: sender as Control);
            if (dialog != ContentDialogResult.Primary) return;

            foreach (var item in mods)
            {
                var mod = item as MinecraftLocalModEntry;
                if (Data.DesktopType == DesktopType.Windows)
                    FileSystem.DeleteFile(mod.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                else
                    File.Delete(mod.Path);
            }

            LoadMods();
        };
        ModManageList.SelectionChanged += (_, _) =>
        {
            SelectedModCount.Text = $"{MainLang.SelectedItem} {ModManageList.SelectedItems.Count}";
        };
        SelectedModCount.Text = $"{MainLang.SelectedItem} 0";

        // Add event handlers for the new buttons
        ModManageList.Loaded += (_, _) =>
        {
            // Find all EnableOrDisableModBtn and DeleteModBtn buttons and attach event handlers
            AttachButtonEventHandlers();
        };
    }

    // private void Translate(MinecraftLocalModEntry entry)
    // {
    // if (string.IsNullOrWhiteSpace(Data.TranslateToken)) return;
    // _ = Task.Run(async () =>
    // {
    //     if (!entry.ShouldTranslateInfoName) return;
    //     try
    //     {
    //         var handler = new HttpClientHandler();
    //         handler.ServerCertificateCustomValidationCallback =
    //             (_, _, _, _) => true;
    //         using var client = new HttpClient(handler);
    //         client.DefaultRequestHeaders.Add("Authorization", Data.TranslateToken);
    //         var response =
    //             await client.PostAsync(
    //                 $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={Data.SettingEntry.Language.Code}&textType=plain",
    //                 new StringContent($"[{{\"Text\": \"{entry.ModInfoName}\"}}]", Encoding.UTF8,
    //                     "application/json"));
    //         var responseContent = await response.Content.ReadAsStringAsync();
    //         var translatedText =
    //             ((JObject)JArray.Parse(responseContent)[0]["translations"][0])["text"].ToString();
    //         entry.DisplayText = $"{translatedText} - {entry.ModInfoName} - {entry.FileName}";
    //     }
    //     catch (Exception e)
    //     {
    //         Logger.Error(e);
    //     }
    // });
    // _ = Task.Run(async () =>
    // {
    //     if (!entry.ShouldTranslateDescription) return;
    //     try
    //     {
    //         var handler = new HttpClientHandler();
    //         handler.ServerCertificateCustomValidationCallback =
    //             (_, _, _, _) => true;
    //         using var client = new HttpClient(handler);
    //         client.DefaultRequestHeaders.Add("Authorization", Data.TranslateToken);
    //         var response =
    //             await client.PostAsync(
    //                 $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={Data.SettingEntry.Language.Code}&textType=plain",
    //                 new StringContent($"[{{\"Text\": \"{entry.ModInfoName}\"}}]", Encoding.UTF8,
    //                     "application/json"));
    //         var responseContent = await response.Content.ReadAsStringAsync();
    //         var translatedText =
    //             ((JObject)JArray.Parse(responseContent)[0]["translations"][0])["text"].ToString();
    //         entry.Description = translatedText.Trim();
    //     }
    //     catch (Exception e)
    //     {
    //         Logger.Error(e);
    //     }
    // });
    // }

    public ModPage()
    {
    }

    public ObservableCollection<MinecraftLocalModEntry> FilteredMods { get; set; } = [];

    public bool IsLoading
    {
        get => _isLoading;
        set => SetField(ref _isLoading, value);
    }

    public string Filter
    {
        get => _filter;
        set => SetField(ref _filter, value);
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }

    private async void LoadMods()
    {
        _mods.Clear();
        IsLoading = true;
        FilterMods();

        var mods = Directory.GetFiles(
            Calculator.GetMinecraftSpecialFolder(_entry, MinecraftSpecialFolder.ModsFolder)
            , "*.*", SearchOption.AllDirectories);
        foreach (var mod in mods)
        {
            MinecraftLocalModEntry? localModEntry = null;
            if (Path.GetExtension(mod) == ".jar")
                localModEntry = new MinecraftLocalModEntry
                {
                    FileName = Path.GetFileName(mod)[..(Path.GetFileName(mod).Length - 4)],
                    IsEnable = true, Path = mod, Callback = LoadMods,
                    DisplayText = Path.GetFileName(mod)[..(Path.GetFileName(mod).Length - 4)]
                };

            if (Path.GetExtension(mod) == ".disabled")
                localModEntry = new MinecraftLocalModEntry
                {
                    FileName = Path.GetFileName(mod)[..(Path.GetFileName(mod).Length - 13)],
                    IsEnable = false, Path = mod, Callback = LoadMods,
                    DisplayText = Path.GetFileName(mod)[..(Path.GetFileName(mod).Length - 13)]
                };

            if (localModEntry == null) continue;

            var (displayName, description) = await GetModInfo(mod);

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                localModEntry.DisplayText = $"{displayName.Trim()} - {localModEntry.FileName}";
                localModEntry.ModInfoName = $"{displayName.Trim()}";
                localModEntry.ShouldTranslateInfoName = true;
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                localModEntry.Description = description.Trim();
                localModEntry.ShouldTranslateDescription = true;
            }
            else
            {
                localModEntry.Description = MainLang.NoDescription;
            }

            if (_mods.All(item => item.Path != localModEntry.Path)) _mods.Add(localModEntry);
            ShortInfo = $"{_entry.Id} / {MainLang.Mod} / 已加载 {_mods.Count} 个模组";

            // Translate(localModEntry);
        }

        IsLoading = false;
        FilterMods();
    }

    private async Task<(string? displayName, string? description)> GetModInfo(string path)
    {
        var result = await Task.Run(async () =>
        {
            try
            {
                if (!File.Exists(path)) return (null, null);
                using var archive = ZipFile.OpenRead(path);
                if (archive.Entries.Count <= 0) return (null, null);
                var type1 = archive.GetEntry("META-INF/mods.toml");
                if (type1 != null)
                    try
                    {
                        await using var entryStream = type1.Open();
                        using var memoryStream = new MemoryStream();
                        await entryStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        var reader = new StreamReader(memoryStream);
                        var text = await reader.ReadToEndAsync();
                        var result = Toml.ToModel(text);

                        if (result.TryGetValue("mods", out var modsObj))
                            if (modsObj is TomlTableArray modsList)
                                foreach (var modTable in modsList.OfType<TomlTable>())
                                {
                                    var displayName = modTable.TryGetValue("displayName", out var nameObj)
                                        ? !string.IsNullOrWhiteSpace(nameObj.ToString()) ? nameObj.ToString() : null
                                        : null;
                                    var description = modTable.TryGetValue("description", out var descObj)
                                        ? !string.IsNullOrWhiteSpace(descObj.ToString()) ? descObj.ToString() : null
                                        : null;
                                    if (!string.IsNullOrWhiteSpace(displayName) ||
                                        !string.IsNullOrWhiteSpace(description))
                                        return (displayName, description);
                                }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }

                var type2 = archive.GetEntry("fabric.mod.json");
                if (type2 != null)
                    try
                    {
                        await using var entryStream = type2.Open();
                        using var memoryStream = new MemoryStream();
                        await entryStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        var reader = new StreamReader(memoryStream);
                        var text = await reader.ReadToEndAsync();
                        var json = JObject.Parse(text);

                        var displayName = !string.IsNullOrWhiteSpace(json["name"]?.ToString())
                            ? json["name"]?.ToString()
                            : null;
                        var description = !string.IsNullOrWhiteSpace(json["description"]?.ToString())
                            ? json["description"]?.ToString()
                            : null;
                        if (!string.IsNullOrWhiteSpace(displayName) ||
                            !string.IsNullOrWhiteSpace(description))
                            return (displayName, description);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }

                var type3 = archive.GetEntry("mcmod.info");
                if (type3 != null)
                    try
                    {
                        await using var entryStream = type3.Open();
                        using var memoryStream = new MemoryStream();
                        await entryStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        var reader = new StreamReader(memoryStream);
                        var text = await reader.ReadToEndAsync();
                        var obj = JArray.Parse(text).FirstOrDefault();
                        if (obj is not JObject o)
                            throw new Exception("mcmod.info is not a valid json array");

                        var displayName = !string.IsNullOrWhiteSpace(o["name"]?.ToString())
                            ? o["name"]?.ToString()
                            : null;
                        var description = !string.IsNullOrWhiteSpace(o["description"]?.ToString())
                            ? o["description"]?.ToString()
                            : null;
                        if (!string.IsNullOrWhiteSpace(displayName) ||
                            !string.IsNullOrWhiteSpace(description))
                            return (displayName, description);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }

                var type4 = archive.GetEntry("META-INF/neoforge.mods.toml");
                if (type4 != null)
                    try
                    {
                        await using var entryStream = type4.Open();
                        using var memoryStream = new MemoryStream();
                        await entryStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        var reader = new StreamReader(memoryStream);
                        var text = await reader.ReadToEndAsync();
                        var result = Toml.ToModel(text);

                        if (result.TryGetValue("mods", out var modsObj))
                            if (modsObj is TomlTableArray modsList)
                                foreach (var modTable in modsList.OfType<TomlTable>())
                                {
                                    var displayName = modTable.TryGetValue("displayName", out var nameObj)
                                        ? !string.IsNullOrWhiteSpace(nameObj.ToString()) ? nameObj.ToString() : null
                                        : null;
                                    var description = modTable.TryGetValue("description", out var descObj)
                                        ? !string.IsNullOrWhiteSpace(descObj.ToString()) ? descObj.ToString() : null
                                        : null;
                                    if (!string.IsNullOrWhiteSpace(displayName) ||
                                        !string.IsNullOrWhiteSpace(description))
                                        return (displayName, description);
                                }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }

                return (null, null);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return (null, null);
            }
        });
        return (result.displayName, result.description);
    }

    private void FilterMods()
    {
        FilteredMods.Clear();
        _mods.Where(item => item.DisplayText.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            .ToList().OrderBy(mod => mod.IsEnable).ToList().ForEach(mod =>
            {
                if (FilteredMods.All(item => item.Path != mod.Path)) FilteredMods.Add(mod);
            });
        NoMatchResultTip.IsVisible = FilteredMods.Count == 0 && !IsLoading;
        SelectedModCount.Text = $"{MainLang.SelectedItem} {ModManageList.SelectedItems.Count}";

        // Reattach event handlers after filtering
        Dispatcher.UIThread.Post(AttachButtonEventHandlers, DispatcherPriority.Background);
    }

    private void AttachButtonEventHandlers()
    {
        // Find all buttons in the ListBox and attach event handlers
        var listBoxItems = ModManageList.GetLogicalDescendants().OfType<ListBoxItem>();
        foreach (var listBoxItem in listBoxItems)
        {
            var enableDisableBtn = listBoxItem.FindNameScope()?.Find("EnableOrDisableModBtn") as Button;
            var deleteBtn = listBoxItem.FindNameScope()?.Find("DeleteModBtn") as Button;

            if (enableDisableBtn != null)
            {
                enableDisableBtn.Click -= OnEnableOrDisableModClick; // Remove existing handler
                enableDisableBtn.Click += OnEnableOrDisableModClick;
            }

            if (deleteBtn != null)
            {
                deleteBtn.Click -= OnDeleteModClick; // Remove existing handler
                deleteBtn.Click += OnDeleteModClick;
            }
        }
    }

    private void OnEnableOrDisableModClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is MinecraftLocalModEntry mod)
        {
            mod.EnableOrDisable();
        }
    }

    private async void OnDeleteModClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is MinecraftLocalModEntry mod)
        {
            await mod.Delete(button);
        }
    }
}