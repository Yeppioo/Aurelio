using System.Collections.ObjectModel;
using Aurelio.Plugin.Minecraft.Classes.Minecraft;
using Aurelio.Plugin.Minecraft.Service.Minecraft.Installer.Core;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Aurelio.Views.Main;
using Aurelio.Views.Overlay;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Installer;
using Ursa.Common;
using Ursa.Controls;
using Ursa.Controls.Options;

namespace Aurelio.Plugin.Minecraft.Views.FetcherPages.Vanilla;

public partial class InstallPreview : PageMixModelBase, IAurelioTabPage, IAurelioNavPage
{
    public readonly VersionManifestEntry _entry;
    private string _validationMessage;
    private bool _isForgeLoading;
    private bool _isFabricLoading;
    private bool _hasOptiFineError;
    private bool _isNeoForgeLoading;
    private bool _isQuiltLoading;
    private bool _isOptiFineLoading;
    private bool _hasForgeError;
    private bool _hasNeoForgeError;
    private string _customInstallationId;
    private bool _hasFabricError;
    private bool _hasQuiltError;

    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    // Illegal filesystem characters
    private static readonly char[] IllegalChars = ['/', '\\', ':', '*', '?', '"', '<', '>', '|'];

    public InstallPreview()
    {
    }

    public InstallPreview(VersionManifestEntry entry)
    {
        _entry = entry;
        _customInstallationId = entry.Id; // Default to Minecraft version name

        InitializeComponent();

        // Find the Root element from XAML
        var rootElement = this.FindControl<Border>("Root");
        RootElement = rootElement;
        DataContext = this;

        InAnimator = new PageLoadingAnimator(rootElement!, new Thickness(0, 60, 0, 0), (0, 1));
        ShortInfo = $"{MainLang.InstallPreview} / {entry.Id}";
        PageInfo = new PageInfoEntry
        {
            Title = entry.Id,
            Icon = StreamGeometry.Parse(
                "M128 128C128 110.3 113.7 96 96 96C78.3 96 64 110.3 64 128L64 464C64 508.2 99.8 544 144 544L544 544C561.7 544 576 529.7 576 512C576 494.3 561.7 480 544 480L144 480C135.2 480 128 472.8 128 464L128 128zM534.6 214.6C547.1 202.1 547.1 181.8 534.6 169.3C522.1 156.8 501.8 156.8 489.3 169.3L384 274.7L326.6 217.4C314.1 204.9 293.8 204.9 281.3 217.4L185.3 313.4C172.8 325.9 172.8 346.2 185.3 358.7C197.8 371.2 218.1 371.2 230.6 358.7L304 285.3L361.4 342.7C373.9 355.2 394.2 355.2 406.7 342.7L534.7 214.7z")
        };

        // Set up retry button event handlers
        SetupRetryButtons();

        // Set up property change notifications for custom ID validation
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(CustomInstallationId))
            {
                ValidateCustomId();
                OnPropertyChanged(nameof(NamingPreview));
                OnPropertyChanged(nameof(ShowNamingWarning));
                OnPropertyChanged(nameof(CanInstall));
            }
        };

        // Start loading mod loader versions asynchronously
        _ = LoadAllModLoadersAsync();
    }

    public static AurelioStaticPageInfo StaticPageInfo { get; } = new()
    {
        Icon = StreamGeometry.Parse(
            "M320 160C320 107 363 64 416 64L454.4 64C503.9 64 544 104.1 544 153.6L544 440C544 515.1 483.1 576 408 576C332.9 576 272 515.1 272 440L272 360C272 337.9 254.1 320 232 320C209.9 320 192 337.9 192 360L192 528C192 554.5 170.5 576 144 576C117.5 576 96 554.5 96 528L96 360C96 284.9 156.9 224 232 224C307.1 224 368 284.9 368 360L368 440C368 462.1 385.9 480 408 480C430.1 480 448 462.1 448 440L448 256L416 256C363 256 320 213 320 160zM464 152C464 138.7 453.3 128 440 128C426.7 128 416 138.7 416 152C416 165.3 426.7 176 440 176C453.3 176 464 165.3 464 152z"),
        Title = MainLang.AutoInstall + " - Minecraft",
        NeedPath = false,
        AutoCreate = true
    };

    public static IAurelioNavPage Create((object sender, object? param) t)
    {
        var root = ((Control)t.sender).GetVisualRoot();
        if (root is TabWindow tabWindow)
        {
            tabWindow.CreateTab(new TabEntry(new VersionSelector()));
            return null;
        }

        App.UiRoot.CreateTab(new TabEntry(new VersionSelector()));
        return null;
    }

    public string ShortInfo { get; set; }
    public Control BottomElement { get; set; }
    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public void OnClose()
    {
    }

    // Properties for data binding
    public VersionManifestEntry MinecraftVersion => _entry;

    // Loading state properties
    public bool IsForgeLoading
    {
        get => _isForgeLoading;
        set => SetField(ref _isForgeLoading, value);
    }

    public bool IsNeoForgeLoading
    {
        get => _isNeoForgeLoading;
        set => SetField(ref _isNeoForgeLoading, value);
    }

    public bool IsFabricLoading
    {
        get => _isFabricLoading;
        set => SetField(ref _isFabricLoading, value);
    }

    public bool IsQuiltLoading
    {
        get => _isQuiltLoading;
        set => SetField(ref _isQuiltLoading, value);
    }

    public bool IsOptiFineLoading
    {
        get => _isOptiFineLoading;
        set => SetField(ref _isOptiFineLoading, value);
    }

    // Error state properties
    public bool HasForgeError
    {
        get => _hasForgeError;
        set => SetField(ref _hasForgeError, value);
    }

    public bool HasNeoForgeError
    {
        get => _hasNeoForgeError;
        set => SetField(ref _hasNeoForgeError, value);
    }

    public bool HasFabricError
    {
        get => _hasFabricError;
        set => SetField(ref _hasFabricError, value);
    }

    public bool HasQuiltError
    {
        get => _hasQuiltError;
        set => SetField(ref _hasQuiltError, value);
    }

    public bool HasOptiFineError
    {
        get => _hasOptiFineError;
        set => SetField(ref _hasOptiFineError, value);
    }

    private ObservableCollection<ForgeInstallEntry> _forgeVersions = [];
    private ObservableCollection<ForgeInstallEntry> _neoForgeVersions = [];
    private ObservableCollection<FabricInstallEntry> _fabricVersions = [];
    private ObservableCollection<QuiltInstallEntry> _quiltVersions = [];
    private ObservableCollection<OptifineInstallEntry> _optiFineVersions = [];

    public ObservableCollection<ForgeInstallEntry> ForgeVersions
    {
        get => _forgeVersions;
        set => SetField(ref _forgeVersions, value);
    }

    public ObservableCollection<ForgeInstallEntry> NeoForgeVersions
    {
        get => _neoForgeVersions;
        set => SetField(ref _neoForgeVersions, value);
    }

    public ObservableCollection<FabricInstallEntry> FabricVersions
    {
        get => _fabricVersions;
        set => SetField(ref _fabricVersions, value);
    }

    public ObservableCollection<QuiltInstallEntry> QuiltVersions
    {
        get => _quiltVersions;
        set => SetField(ref _quiltVersions, value);
    }

    public ObservableCollection<OptifineInstallEntry> OptiFineVersions
    {
        get => _optiFineVersions;
        set => SetField(ref _optiFineVersions, value);
    }

    private bool _hasValidationError;
    private ForgeInstallEntry? _selectedForge;
    private ForgeInstallEntry? _selectedNeoForge;
    private OptifineInstallEntry? _selectedOptiFine;
    private FabricInstallEntry? _selectedFabric;
    private QuiltInstallEntry? _selectedQuilt;

    public bool IsForgeEmpty => !IsForgeLoading && !HasForgeError && !ForgeVersions.Any();
    public bool IsNeoForgeEmpty => !IsNeoForgeLoading && !HasNeoForgeError && !NeoForgeVersions.Any();
    public bool IsFabricEmpty => !IsFabricLoading && !HasFabricError && !FabricVersions.Any();
    public bool IsQuiltEmpty => !IsQuiltLoading && !HasQuiltError && !QuiltVersions.Any();
    public bool IsOptiFineEmpty => !IsOptiFineLoading && !HasOptiFineError && !OptiFineVersions.Any();

    // Custom installation ID properties
    public string CustomInstallationId
    {
        get => _customInstallationId;
        set => SetField(ref _customInstallationId, value);
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        set => SetField(ref _validationMessage, value);
    }

    public bool HasValidationError
    {
        get => _hasValidationError;
        set => SetField(ref _hasValidationError, value);
    }

    public ForgeInstallEntry? SelectedForge
    {
        get => _selectedForge;
        set
        {
            SetField(ref _selectedForge, value);
            OnPropertyChanged(nameof(SelectedForgeVersion));
            OnPropertyChanged(nameof(HasSelectedForge));

            _selectedFabric = null;
            _selectedNeoForge = null;
            _selectedQuilt = null;
            OnPropertyChanged(nameof(SelectedFabric));
            OnPropertyChanged(nameof(SelectedNeoForge));
            OnPropertyChanged(nameof(SelectedQuilt));

            OnPropertyChanged(nameof(SelectedQuiltVersion));
            OnPropertyChanged(nameof(HasSelectedQuilt));
            OnPropertyChanged(nameof(SelectedNeoForgeVersion));
            OnPropertyChanged(nameof(HasSelectedNeoForge));
            OnPropertyChanged(nameof(SelectedFabricVersion));
            OnPropertyChanged(nameof(HasSelectedFabric));

            OnPropertyChanged(nameof(HasAnySelection));
            OnPropertyChanged(nameof(NamingPreview));
            OnPropertyChanged(nameof(ShowNamingWarning));
        }
    }

    public ForgeInstallEntry? SelectedNeoForge
    {
        get => _selectedNeoForge;
        set
        {
            SetField(ref _selectedNeoForge, value);
            OnPropertyChanged(nameof(SelectedNeoForgeVersion));
            OnPropertyChanged(nameof(HasSelectedNeoForge));

            _selectedFabric = null;
            _selectedForge = null;
            _selectedQuilt = null;
            OnPropertyChanged(nameof(SelectedFabric));
            OnPropertyChanged(nameof(SelectedForge));
            OnPropertyChanged(nameof(SelectedQuilt));

            OnPropertyChanged(nameof(SelectedForgeVersion));
            OnPropertyChanged(nameof(HasSelectedForge));
            OnPropertyChanged(nameof(SelectedQuiltVersion));
            OnPropertyChanged(nameof(HasSelectedQuilt));
            OnPropertyChanged(nameof(SelectedFabricVersion));
            OnPropertyChanged(nameof(HasSelectedFabric));

            OnPropertyChanged(nameof(HasAnySelection));
            OnPropertyChanged(nameof(NamingPreview));
            OnPropertyChanged(nameof(ShowNamingWarning));
        }
    }

    public FabricInstallEntry? SelectedFabric
    {
        get => _selectedFabric;
        set
        {
            SetField(ref _selectedFabric, value);
            OnPropertyChanged(nameof(SelectedFabricVersion));
            OnPropertyChanged(nameof(HasSelectedFabric));

            _selectedOptiFine = null;
            _selectedNeoForge = null;
            _selectedForge = null;
            _selectedQuilt = null;
            OnPropertyChanged(nameof(SelectedOptiFine));
            OnPropertyChanged(nameof(SelectedNeoForge));
            OnPropertyChanged(nameof(SelectedForge));
            OnPropertyChanged(nameof(SelectedQuilt));

            OnPropertyChanged(nameof(SelectedForgeVersion));
            OnPropertyChanged(nameof(HasSelectedForge));
            OnPropertyChanged(nameof(SelectedQuiltVersion));
            OnPropertyChanged(nameof(HasSelectedQuilt));
            OnPropertyChanged(nameof(SelectedNeoForgeVersion));
            OnPropertyChanged(nameof(HasSelectedNeoForge));
            OnPropertyChanged(nameof(SelectedOptiFineVersion));
            OnPropertyChanged(nameof(HasSelectedOptiFine));

            OnPropertyChanged(nameof(HasAnySelection));
            OnPropertyChanged(nameof(NamingPreview));
            OnPropertyChanged(nameof(ShowNamingWarning));
        }
    }

    public QuiltInstallEntry? SelectedQuilt
    {
        get => _selectedQuilt;
        set
        {
            SetField(ref _selectedQuilt, value);
            OnPropertyChanged(nameof(SelectedQuiltVersion));
            OnPropertyChanged(nameof(HasSelectedQuilt));

            _selectedOptiFine = null;
            _selectedFabric = null;
            _selectedNeoForge = null;
            _selectedForge = null;
            OnPropertyChanged(nameof(SelectedOptiFine));
            OnPropertyChanged(nameof(SelectedNeoForge));
            OnPropertyChanged(nameof(SelectedForge));
            OnPropertyChanged(nameof(SelectedFabric));

            OnPropertyChanged(nameof(SelectedForgeVersion));
            OnPropertyChanged(nameof(HasSelectedForge));
            OnPropertyChanged(nameof(SelectedNeoForgeVersion));
            OnPropertyChanged(nameof(HasSelectedNeoForge));
            OnPropertyChanged(nameof(SelectedFabricVersion));
            OnPropertyChanged(nameof(HasSelectedFabric));
            OnPropertyChanged(nameof(SelectedOptiFineVersion));
            OnPropertyChanged(nameof(HasSelectedOptiFine));

            OnPropertyChanged(nameof(HasAnySelection));
            OnPropertyChanged(nameof(NamingPreview));
            OnPropertyChanged(nameof(ShowNamingWarning));
        }
    }

    public OptifineInstallEntry? SelectedOptiFine
    {
        get => _selectedOptiFine;
        set
        {
            SetField(ref _selectedOptiFine, value);
            OnPropertyChanged(nameof(SelectedOptiFineVersion));
            OnPropertyChanged(nameof(HasSelectedOptiFine));

            _selectedFabric = null;
            _selectedQuilt = null;
            OnPropertyChanged(nameof(SelectedQuilt));
            OnPropertyChanged(nameof(SelectedFabric));

            OnPropertyChanged(nameof(SelectedFabricVersion));
            OnPropertyChanged(nameof(HasSelectedFabric));

            OnPropertyChanged(nameof(HasAnySelection));
            OnPropertyChanged(nameof(NamingPreview));
            OnPropertyChanged(nameof(ShowNamingWarning));
        }
    }


    public bool HasSelectedForge => _selectedForge != null;
    public bool HasSelectedNeoForge => _selectedNeoForge != null;
    public bool HasSelectedFabric => _selectedFabric != null;
    public bool HasSelectedQuilt => _selectedQuilt != null;
    public bool HasSelectedOptiFine => _selectedOptiFine != null;
    public string SelectedForgeVersion => _selectedForge?.ForgeVersion ?? "None";
    public string SelectedNeoForgeVersion => _selectedNeoForge?.ForgeVersion ?? "None";
    public string SelectedFabricVersion => _selectedFabric?.BuildVersion ?? "None";
    public string SelectedQuiltVersion => _selectedQuilt?.BuildVersion ?? "None";

    public string SelectedOptiFineVersion => _selectedOptiFine != null
        ? $"{_selectedOptiFine.Type} {_selectedOptiFine.Patch}"
        : "None";

    public bool HasAnySelection => HasSelectedForge || HasSelectedNeoForge || HasSelectedFabric || HasSelectedQuilt ||
                                   HasSelectedOptiFine;

    // Naming preview properties

    public string AdditionInstall
    {
        get
        {
            if (!HasAnySelection)
                return MainLang.NoAdditionalInstall;

            var parts = new List<string> { MinecraftVersion.Id };

            if (HasSelectedForge) parts.Add($"Forge {SelectedForgeVersion}");
            if (HasSelectedNeoForge) parts.Add($"NeoForge {SelectedNeoForgeVersion}");
            if (HasSelectedFabric) parts.Add($"Fabric {SelectedFabricVersion}");
            if (HasSelectedQuilt) parts.Add($"Quilt {SelectedQuiltVersion}");
            if (HasSelectedOptiFine) parts.Add($"OptiFine {SelectedOptiFineVersion}");


            return string.Join(", ", parts);
        }
    }

    public string NamingPreview
    {
        get
        {
            OnPropertyChanged(nameof(AdditionInstall));
            if (!HasAnySelection || CustomInstallationId != MinecraftVersion.Id)
                return string.Empty;

            var parts = new List<string> { MinecraftVersion.Id };

            if (HasSelectedForge) parts.Add($"Forge {SelectedForgeVersion}");
            if (HasSelectedNeoForge) parts.Add($"NeoForge {SelectedNeoForgeVersion}");
            if (HasSelectedFabric) parts.Add($"Fabric {SelectedFabricVersion}");
            if (HasSelectedQuilt) parts.Add($"Quilt {SelectedQuiltVersion}");
            if (HasSelectedOptiFine) parts.Add($"OptiFine {SelectedOptiFineVersion}");


            return string.Join(" ", parts);
        }
    }

    public bool ShowNamingWarning => !string.IsNullOrEmpty(NamingPreview);

    // Installation properties
    public bool CanInstall => !HasValidationError && !string.IsNullOrWhiteSpace(CustomInstallationId);

    public string InstallButtonMessage
    {
        get
        {
            if (HasValidationError) return "Fix validation errors to continue";
            if (string.IsNullOrWhiteSpace(CustomInstallationId)) return "Please enter installation ID";
            if (!HasAnySelection) return "Select mod loaders to install (optional)";
            return "Ready to install";
        }
    }

    public bool HasInstallMessage => !string.IsNullOrEmpty(InstallButtonMessage);

    // Core methods
    private async Task LoadAllModLoadersAsync()
    {
        var tasks = new[]
        {
            LoadForgeVersionsAsync(),
            LoadNeoForgeVersionsAsync(),
            LoadFabricVersionsAsync(),
            LoadQuiltVersionsAsync(),
            LoadOptiFineVersionsAsync()
        };

        await Task.WhenAll(tasks);
    }

    private async Task LoadOptiFineVersionsAsync()
    {
        IsOptiFineLoading = true;
        HasOptiFineError = false;
        OnPropertyChanged(nameof(IsOptiFineEmpty));
        try
        {
            var li = await OptifineInstaller.EnumerableOptifineAsync(_entry.Id);
            OptiFineVersions.Clear();
            foreach (var x in li)
            {
                OptiFineVersions.Add(x);
            }
        }
        catch (Exception)
        {
            HasOptiFineError = true;
        }
        finally
        {
            IsOptiFineLoading = false;
            OnPropertyChanged(nameof(IsOptiFineEmpty));
        }
    }

    private async Task LoadQuiltVersionsAsync()
    {
        IsQuiltLoading = true;
        HasQuiltError = false;
        OnPropertyChanged(nameof(IsQuiltEmpty));
        try
        {
            var li = await QuiltInstaller.EnumerableQuiltAsync(_entry.Id);
            QuiltVersions.Clear();
            foreach (var x in li)
            {
                QuiltVersions.Add(x);
            }
        }
        catch (Exception)
        {
            HasQuiltError = true;
        }
        finally
        {
            IsQuiltLoading = false;
            OnPropertyChanged(nameof(IsQuiltEmpty));
        }
    }

    private async Task LoadFabricVersionsAsync()
    {
        IsFabricLoading = true;
        HasFabricError = false;
        OnPropertyChanged(nameof(IsFabricEmpty));
        try
        {
            var li = await FabricInstaller.EnumerableFabricAsync(_entry.Id);
            FabricVersions.Clear();
            foreach (var x in li)
            {
                FabricVersions.Add(x);
            }
        }
        catch (Exception)
        {
            HasFabricError = true;
        }
        finally
        {
            IsFabricLoading = false;
            OnPropertyChanged(nameof(IsFabricEmpty));
        }
    }

    private async Task LoadNeoForgeVersionsAsync()
    {
        IsNeoForgeLoading = true;
        HasNeoForgeError = false;
        OnPropertyChanged(nameof(IsNeoForgeEmpty));
        try
        {
            var li = await ForgeInstaller.EnumerableForgeAsync(_entry.Id, true);
            NeoForgeVersions.Clear();
            foreach (var x in li)
            {
                NeoForgeVersions.Add(x);
            }
        }
        catch (Exception)
        {
            HasNeoForgeError = true;
        }
        finally
        {
            IsNeoForgeLoading = false;
            OnPropertyChanged(nameof(IsNeoForgeEmpty));
        }
    }

    private async Task LoadForgeVersionsAsync()
    {
        IsForgeLoading = true;
        HasForgeError = false;
        OnPropertyChanged(nameof(IsForgeEmpty));
        try
        {
            var li = await ForgeInstaller.EnumerableForgeAsync(_entry.Id);
            ForgeVersions.Clear();
            foreach (var x in li)
            {
                ForgeVersions.Add(x);
            }
        }
        catch (Exception)
        {
            HasForgeError = true;
        }
        finally
        {
            IsForgeLoading = false;
            OnPropertyChanged(nameof(IsForgeEmpty));
        }
    }

    private void ValidateCustomId()
    {
        if (string.IsNullOrWhiteSpace(CustomInstallationId))
        {
            HasValidationError = true;
            ValidationMessage = MainLang.VersionIdCannotBeEmpty;
            return;
        }

        // Check for illegal characters
        if (CustomInstallationId.IndexOfAny(IllegalChars) >= 0)
        {
            HasValidationError = true;
            ValidationMessage = $"{MainLang.IncludeSpecialWord}: / \\ : * ? \" < > |";
            return;
        }

        // Check for reserved names
        if (ReservedNames.Contains(CustomInstallationId))
        {
            HasValidationError = true;
            ValidationMessage = "不可命名为特殊名称";
            return;
        }

        // Check for names ending with period or space
        if (CustomInstallationId.EndsWith('.') || CustomInstallationId.EndsWith(' '))
        {
            HasValidationError = true;
            ValidationMessage = "不可以点或空格结束";
            return;
        }

        HasValidationError = false;
        ValidationMessage = string.Empty;
    }

    private void SetupRetryButtons()
    {
        // Find retry buttons and set up event handlers
        // Note: These buttons might not exist initially since they're only visible on error
        try
        {
            var retryOptiFineBtn = this.FindControl<Button>("RetryOptiFineBtn");
            var retryForgeBtn = this.FindControl<Button>("RetryForgeBtn");
            var retryNeoForgeBtn = this.FindControl<Button>("RetryNeoForgeBtn");
            var retryFabricBtn = this.FindControl<Button>("RetryFabricBtn");
            var retryQuiltBtn = this.FindControl<Button>("RetryQuiltBtn");


            if (retryOptiFineBtn != null)
                retryOptiFineBtn.Click += async (_, _) => await LoadOptiFineVersionsAsync();

            if (retryForgeBtn != null)
                retryForgeBtn.Click += async (_, _) => await LoadForgeVersionsAsync();

            if (retryNeoForgeBtn != null)
                retryNeoForgeBtn.Click += async (_, _) => await LoadNeoForgeVersionsAsync();

            if (retryFabricBtn != null)
                retryFabricBtn.Click += async (_, _) => await LoadFabricVersionsAsync();

            if (retryQuiltBtn != null)
                retryQuiltBtn.Click += async (_, _) => await LoadQuiltVersionsAsync();
        }
        catch
        {
            // Buttons might not be available yet, that's okay
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ReturnToListRoot_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var root = this.GetVisualRoot();
        if (root is TabWindow tabWindow)
        {
            tabWindow.CreateTab(new TabEntry(new VersionSelector()));
            HostTab.Close();
            return;
        }

        App.UiRoot.CreateTab(new TabEntry(new VersionSelector()));
        HostTab.Close();
    }

    private async void BeginInstallBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        var cid = ShowNamingWarning ? NamingPreview : CustomInstallationId;
        if ((SelectedOptiFine != null || SelectedForge != null || SelectedNeoForge != null)
            && (MinecraftPluginData.MinecraftPluginSettingEntry.JavaRuntimes.Count == 0 ||
                MinecraftPluginData.MinecraftPluginSettingEntry.JavaRuntimes.All(x => x.JavaFolder == null)))
        {
            Notice(MainLang.JavaRuntimeTip, NotificationType.Error);
            return;
        }

        var com = new ComboBox
        {
            ItemsSource = MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftFolderEntries,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemTemplate = new FuncDataTemplate<RecordMinecraftFolderEntry>((item, _)
                => new TextBlock
                    { Text = item == null ? "Error" : $"[{item.Name}] {item.Path}", TextWrapping = TextWrapping.Wrap }),
            SelectedItem = MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftFolderEntries.FirstOrDefault()
        };
        var cr = await ShowDialogAsync("选择安装目录", p_content: com,
            b_primary: MainLang.Ok, b_cancel: MainLang.Cancel);

        if (cr == ContentDialogResult.None) return;

        if (HasAnySelection && Directory.Exists(Path.Combine((com.SelectedItem as RecordMinecraftFolderEntry).Path,
                "versions", cid)))
        {
            var cr1 = await ShowDialogAsync($"{MainLang.FolderAlreadyExists}: {cid}",
                b_cancel: MainLang.Cancel, b_primary: MainLang.Delete);
            if (cr1 == ContentDialogResult.None)
                return;
            Directory.Delete(Path.Combine((com.SelectedItem as RecordMinecraftFolderEntry).Path, "versions",
                cid), true);
        }

        _ = Entrance.Main(_entry, (com.SelectedItem as RecordMinecraftFolderEntry).Path, cid,
            SelectedOptiFine, SelectedFabric, SelectedForge ?? SelectedNeoForge ?? null, SelectedQuilt);

        _ = OpenTaskDrawer(GetHostId(this));

        var root = this.GetVisualRoot();
        if (root is TabWindow tabWindow)
        {
            tabWindow.CreateTab(new TabEntry(new VersionSelector()));
            HostTab.Close();
            return;
        }

        App.UiRoot.CreateTab(new TabEntry(new VersionSelector()));
        HostTab.Close();
    }
}