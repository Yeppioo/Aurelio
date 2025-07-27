using System.IO.Compression;
using Aurelio.Plugin.Minecraft.Classes.Enum.Minecraft;
using Aurelio.Plugin.Minecraft.Classes.Minecraft;
using MinecraftSpecialFolder = Aurelio.Plugin.Minecraft.Classes.Enum.Minecraft.MinecraftSpecialFolder;
using Aurelio.Plugin.Minecraft.Service.Minecraft;
using Aurelio.Plugin.Minecraft.Views.MinecraftInstancePages;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Ursa.Controls;

namespace Aurelio.Plugin.Minecraft.Views;

public partial class MinecraftInstancePage : PageMixModelBase, IAurelioTabPage
{
    private bool _fl = true;
    private SelectionListItem _selectedItem;

    public MinecraftInstancePage(RecordMinecraftEntry entry)
    {
        Entry = entry;
        OverViewPage = new OverViewPage(Entry);
        ModPage = new ModPage(Entry.MlEntry);
        ResourcePackPage = new ResourcePackPage(Entry.MlEntry);
        SavePage = new SavePage(Entry.MlEntry);
        ShaderPackPage = new ShaderPackPage(Entry.MlEntry);
        ScreenshotPage = new ScreenshotPage(Entry.MlEntry);
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        PageInfo = new PageInfoEntry
        {
            Title = Entry.Id,
            Icon = Icons.Seedling
        };
        Loaded += (_, _) =>
        {
            if (!_fl) return;
            _fl = false;
            SelectedItem = Nav.Items[0] as SelectionListItem;
        };
        PropertyChanged += (s, e) =>
        {
            if (SelectedItem == null || e.PropertyName != nameof(SelectedItem)) return;
            if (SelectedItem.Tag is not IAurelioPage page) return;
            page.RootElement.IsVisible = false;
            page.InAnimator.Animate();
        };
        AddHandler(DragDrop.DropEvent, DropHandler);

        // Add event handlers for buttons
        AttachEventHandlers();
    }

    private async Task DropHandler(object? sender, DragEventArgs e)
    {
        e.Handled = true;
        if (e is null) return;
        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files == null) return;
            var storageItems = files as IStorageItem[] ?? files.ToArray();
            var jar = storageItems.Where(a => Path.GetExtension(a.Path.LocalPath) == ".jar").ToArray();
            var zip = storageItems.Where(a => Path.GetExtension(a.Path.LocalPath) == ".zip").ToArray();

            // Handle JAR files
            if (jar.Length > 0)
            {
                await HandleJarFiles(jar);
            }

            // Handle ZIP files
            if (zip.Length > 0)
            {
                await HandleZipFiles(zip);
            }
        }
    }

    private async Task HandleJarFiles(IStorageItem[] jarFiles)
    {
        if (jarFiles.Length == 1)
        {
            var cr = await ShowDialogAsync(MainLang.Import,
                $"{MainLang.Import} → {Entry.ParentMinecraftFolder.Name}/{Entry.Id}\n\n" +
                $"{MainLang.ImportJarAsMod.Replace("{file}", Path.GetFileName(jarFiles[0].Path.LocalPath))
                    .Replace("{mc}", Entry.Id)}",
                b_primary: MainLang.Import, b_cancel: MainLang.Cancel);
            if (cr != ContentDialogResult.Primary) return;
            var s = await Public.Module.IO.Local.Setter.CopyFileWithDialog(jarFiles[0].Path.LocalPath,
                Path.Combine(Calculator.GetMinecraftSpecialFolder(Entry.MlEntry,
                    MinecraftSpecialFolder.ModsFolder), Path.GetFileName(jarFiles[0].Path.LocalPath)));
            if (!s) return;
            Notice(MainLang.ImportFinish, NotificationType.Success);
        }
        else if (jarFiles.Length > 1)
        {
            List<string> list = [];
            list.AddRange(jarFiles.Select(x => x.Path.LocalPath));

            foreach (var itemPath in jarFiles.Select(x => x.Path.LocalPath))
            {
                string text = list.Aggregate(string.Empty,
                    (current, path) => current + $"• {Path.GetFileName(path)}\n");
                var cr = await ShowDialogAsync(MainLang.Import,
                    $"{MainLang.Import} → {Entry.ParentMinecraftFolder.Name}/{Entry.Id}\n\n" +
                    $"{MainLang.ImportJarAsMod.Replace("{file}", Path.GetFileName(itemPath)
                        ).Replace("{mc}", Entry.Id)}\n\n" +
                    $"{MainLang.TodoList}: \n{text}",
                    b_primary: MainLang.Import, b_cancel: MainLang.Cancel, b_secondary: MainLang.AllImport);

                if (cr == ContentDialogResult.Primary)
                {
                    var s = await Public.Module.IO.Local.Setter.CopyFileWithDialog(itemPath,
                        Path.Combine(Calculator.GetMinecraftSpecialFolder(Entry.MlEntry,
                            MinecraftSpecialFolder.ModsFolder), Path.GetFileName(itemPath)));
                    if (!s) continue;
                    Notice(MainLang.ImportFinish, NotificationType.Success);
                    list.Remove(itemPath);
                }

                if (cr == ContentDialogResult.Secondary)
                {
                    foreach (var itemPath1 in list)
                    {
                        var s = await Public.Module.IO.Local.Setter.CopyFileWithDialog(itemPath1,
                            Path.Combine(Calculator.GetMinecraftSpecialFolder(Entry.MlEntry,
                                MinecraftSpecialFolder.ModsFolder), Path.GetFileName(itemPath1)));
                    }
                    Notice(MainLang.ImportFinish, NotificationType.Success);
                    break;
                }

                if (cr == ContentDialogResult.None)
                {
                    break;
                }
            }
        }
    }

    private async Task HandleZipFiles(IStorageItem[] zipFiles)
    {
        try
        {
            // Validate ZIP files
            var validZipFiles = new List<IStorageItem>();
            foreach (var zipFile in zipFiles)
            {
                if (IsValidZipFile(zipFile.Path.LocalPath))
                {
                    validZipFiles.Add(zipFile);
                }
                else
                {
                    Notice($"{MainLang.ImportFailed}: {Path.GetFileName(zipFile.Path.LocalPath)}", NotificationType.Error);
                }
            }

            if (validZipFiles.Count == 0) return;

            // Import files based on count
            if (validZipFiles.Count == 1)
            {
                // Single file: show dialog and import
                var result = await ShowZipImportDialogWithOptions(validZipFiles.ToArray());
                if (result.DialogResult == ContentDialogResult.None || result.SelectedFolder == null) return;
                await ImportZipFile(validZipFiles[0].Path.LocalPath, result.SelectedFolder.Value);
            }
            else
            {
                // Multiple files: use new sequential import workflow
                await HandleMultipleZipFilesSequentially(validZipFiles);
            }
        }
        catch (Exception ex)
        {
            Notice($"{MainLang.ImportFailed}: {ex.Message}", NotificationType.Error);
        }
    }

    private async Task HandleMultipleZipFilesSequentially(List<IStorageItem> zipFiles)
    {
        var remainingFiles = new List<IStorageItem>(zipFiles);

        // Process files one by one, always showing target selection
        while (remainingFiles.Count > 0)
        {
            var result = await ShowZipImportDialogWithOptions(remainingFiles.ToArray());

            if (result.DialogResult == ContentDialogResult.None || result.SelectedFolder == null) // User cancelled or no folder selected
                break;

            if (result.DialogResult == ContentDialogResult.Primary) // Import single file
            {
                await ImportZipFile(remainingFiles[0].Path.LocalPath, result.SelectedFolder.Value);
                remainingFiles.RemoveAt(0);
            }
            else if (result.DialogResult == ContentDialogResult.Secondary) // Import all remaining
            {
                await ImportMultipleZipFiles(remainingFiles.ToArray(), result.SelectedFolder.Value);
                break;
            }
        }
    }

    private async Task<(ContentDialogResult DialogResult, MinecraftSpecialFolder? SelectedFolder)> ShowZipImportDialogWithOptions(IStorageItem[] zipFiles)
    {
        var comboBox = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 300
        };

        // Populate ComboBox with valid import folders (only ResourcePacks and ShaderPacks)
        var validFolders = GetValidZipImportFolders();
        foreach (var folder in validFolders)
        {
            var item = new ComboBoxItem
            {
                Content = GetLocalizedFolderName(folder),
                Tag = folder
            };
            comboBox.Items.Add(item);
        }
        comboBox.SelectedIndex = 0; // Default to first option

        // Create file list text
        var fileList = string.Join("\n", zipFiles.Select(f => $"• {Path.GetFileName(f.Path.LocalPath)}"));

        // Create header message
        var headerMessage = $"{MainLang.Import} → {Entry.ParentMinecraftFolder.Name}/{Entry.Id}\n\n" +
                           $"{MainLang.CurrentItem}: {Path.GetFileName(zipFiles.First().Path.LocalPath)}";

        // Create content panel with ComboBox above file list
        var content = new StackPanel
        {
            Spacing = 15,
            Children =
            {
                new SelectableTextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Text = headerMessage
                },
                new StackPanel
                {
                    Spacing = 5,
                    Children =
                    {
                        new TextBlock { Text = MainLang.SelectResourceType },
                        comboBox
                    }
                },
                new StackPanel
                {
                    Spacing = 5,
                    Children =
                    {
                        new TextBlock { Text = $"{MainLang.TodoHandleFile} ({zipFiles.Length}):" },
                        new SelectableTextBlock
                        {
                            TextWrapping = TextWrapping.Wrap,
                            Text = fileList
                        }
                    }
                }
            }
        };

        // Show dialog with Import and Import All options for multiple files
        var result = await ShowDialogAsync(
            MainLang.Import,
            p_content: content,
            b_primary: MainLang.Import, // Import single file
            b_cancel: MainLang.Cancel,
            b_secondary: zipFiles.Length > 1 ? MainLang.AllImport : string.Empty); // Import all for multiple files

        var selectedItem = comboBox.SelectedItem as ComboBoxItem;
        var selectedFolder = selectedItem?.Tag as MinecraftSpecialFolder?;

        return (result, selectedFolder);
    }



    private List<MinecraftSpecialFolder> GetValidZipImportFolders()
    {
        return new List<MinecraftSpecialFolder>
        {
            MinecraftSpecialFolder.ResourcePacksFolder,
            MinecraftSpecialFolder.ShaderPacksFolder
        };
    }

    private string GetLocalizedFolderName(MinecraftSpecialFolder folder)
    {
        return folder switch
        {
            MinecraftSpecialFolder.ResourcePacksFolder => MainLang.ResourcePacksFolder,
            MinecraftSpecialFolder.ShaderPacksFolder => MainLang.ShaderPacksFolder,
            _ => folder.ToString()
        };
    }

    private bool IsValidZipFile(string filePath)
    {
        try
        {
            using var archive = ZipFile.OpenRead(filePath);
            return archive.Entries.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task ImportZipFile(string zipPath, MinecraftSpecialFolder targetFolder)
    {
        var targetPath = Calculator.GetMinecraftSpecialFolder(Entry.MlEntry, targetFolder);
        var fileName = Path.GetFileName(zipPath);
        var destinationPath = Path.Combine(targetPath, fileName);

        var success = await Public.Module.IO.Local.Setter.CopyFileWithDialog(zipPath, destinationPath);
        if (success)
        {
            Notice(MainLang.ImportFinish, NotificationType.Success);
        }
    }

    private async Task ImportMultipleZipFiles(IStorageItem[] zipFiles, MinecraftSpecialFolder targetFolder)
    {
        var targetPath = Calculator.GetMinecraftSpecialFolder(Entry.MlEntry, targetFolder);
        var successCount = 0;

        foreach (var zipFile in zipFiles)
        {
            var fileName = Path.GetFileName(zipFile.Path.LocalPath);
            var destinationPath = Path.Combine(targetPath, fileName);

            var success = await Public.Module.IO.Local.Setter.CopyFileWithDialog(zipFile.Path.LocalPath, destinationPath);
            if (success)
            {
                successCount++;
            }
        }

        if (successCount > 0)
        {
            Notice($"{MainLang.ImportFinish} ({successCount}/{zipFiles.Length})", NotificationType.Success);
        }
    }

    public MinecraftInstancePage()
    {
    }

    public RecordMinecraftEntry Entry { get; }

    public SelectionListItem SelectedItem
    {
        get => _selectedItem;
        set => SetField(ref _selectedItem, value);
    }

    public OverViewPage OverViewPage { get; }
    public ModPage ModPage { get; }
    public SavePage SavePage { get; }
    public ShaderPackPage ShaderPackPage { get; }
    public ScreenshotPage ScreenshotPage { get; }
    public ResourcePackPage ResourcePackPage { get; }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public void OnClose()
    {
        // 清理各个子页面的资源
        ScreenshotPage?.OnClose();

        // // 清理图片缓存以释放内存
        // ImageCache.ClearCache();
    }

    public void OpenFolder(MinecraftSpecialFolder folder)
    {
        App.TopLevel.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(
            Calculator.GetMinecraftSpecialFolder(Entry.MlEntry, folder)));
    }

    private void AttachEventHandlers()
    {
        // Attach event handlers for buttons and menu items
        if (OpenFolderSplitBtn != null)
        {
            OpenFolderSplitBtn.Click += OnOpenFolderSplitBtnClick;
        }

        if (LaunchBtn != null)
        {
            LaunchBtn.Click += OnLaunchBtnClick;
        }

        // Attach event handlers for menu items
        if (VersionFolderMenuItem != null)
        {
            VersionFolderMenuItem.Click += (s, e) => OnOpenFolderMenuItemClick(MinecraftSpecialFolder.InstanceFolder);
        }

        if (ModsFolderMenuItem != null)
        {
            ModsFolderMenuItem.Click += (s, e) => OnOpenFolderMenuItemClick(MinecraftSpecialFolder.ModsFolder);
        }

        if (SavesFolderMenuItem != null)
        {
            SavesFolderMenuItem.Click += (s, e) => OnOpenFolderMenuItemClick(MinecraftSpecialFolder.SavesFolder);
        }

        if (ResourcePacksFolderMenuItem != null)
        {
            ResourcePacksFolderMenuItem.Click += (s, e) => OnOpenFolderMenuItemClick(MinecraftSpecialFolder.ResourcePacksFolder);
        }

        if (ShaderPacksFolderMenuItem != null)
        {
            ShaderPacksFolderMenuItem.Click += (s, e) => OnOpenFolderMenuItemClick(MinecraftSpecialFolder.ShaderPacksFolder);
        }

        if (ScreenshotsFolderMenuItem != null)
        {
            ScreenshotsFolderMenuItem.Click += (s, e) => OnOpenFolderMenuItemClick(MinecraftSpecialFolder.ScreenshotsFolder);
        }
    }

    private void OnOpenFolderSplitBtnClick(object? sender, RoutedEventArgs e)
    {
        // Default action for SplitButton click - open instance folder (parameter 0)
        OpenFolder(MinecraftSpecialFolder.InstanceFolder);
    }

    private void OnLaunchBtnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Control control)
        {
            Entry.Launch(control);
        }
    }

    private void OnOpenFolderMenuItemClick(MinecraftSpecialFolder folder)
    {
        OpenFolder(folder);
    }
}