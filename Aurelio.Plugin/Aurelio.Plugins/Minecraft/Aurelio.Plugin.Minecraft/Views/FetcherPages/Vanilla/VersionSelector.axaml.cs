using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Input;
using MinecraftLaunch.Base.Models.Network;
using Aurelio.Public.Classes.Entries;
using Aurelio.Views.Main;
using Aurelio.Views.Main.Pages;
using Avalonia.Media;
using Avalonia.VisualTree;
using MinecraftLaunch.Components.Installer;

namespace Aurelio.Plugin.Minecraft.Views.FetcherPages.Vanilla;

public partial class VersionSelector : PageMixModelBase, IAurelioTabPage, IAurelioNavPage
{
    public new MinecraftPluginData Data => MinecraftPluginData.Instance;

    // Loading and error state properties
    private bool _isLoading = true;
    private bool _hasError = false;
    private string _errorMessage = string.Empty;

    // Latest version properties
    private VersionManifestEntry _latestSnapshot = new() { Id = MainLang.Loading, ReleaseTime = DateTime.MinValue };
    private VersionManifestEntry _latestRelease = new() { Id = MainLang.Loading, ReleaseTime = DateTime.MinValue };

    // Search text properties
    private string _allVersionsSearchText = string.Empty;
    private string _releaseVersionsSearchText = string.Empty;
    private string _snapshotVersionsSearchText = string.Empty;
    private string _oldVersionsSearchText = string.Empty;

    // Filtered collections
    private ObservableCollection<VersionManifestEntry> _filteredAllVersions = [];
    private ObservableCollection<VersionManifestEntry> _filteredReleaseVersions = [];
    private ObservableCollection<VersionManifestEntry> _filteredSnapshotVersions = [];
    private ObservableCollection<VersionManifestEntry> _filteredOldVersions = [];

    // Version collections
    private List<VersionManifestEntry> _allVersions = [];
    private List<VersionManifestEntry> _releaseVersions = [];
    private List<VersionManifestEntry> _snapshotVersions = [];
    private List<VersionManifestEntry> _oldVersions = [];

    public VersionSelector()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
        ShortInfo = $"{MainLang.AutoInstall} / {MainLang.Version}";
        PageInfo = new PageInfoEntry
        {
            Title = MainLang.AutoInstall,
            Icon = StreamGeometry.Parse(
                "M320 160C320 107 363 64 416 64L454.4 64C503.9 64 544 104.1 544 153.6L544 440C544 515.1 483.1 576 408 576C332.9 576 272 515.1 272 440L272 360C272 337.9 254.1 320 232 320C209.9 320 192 337.9 192 360L192 528C192 554.5 170.5 576 144 576C117.5 576 96 554.5 96 528L96 360C96 284.9 156.9 224 232 224C307.1 224 368 284.9 368 360L368 440C368 462.1 385.9 480 408 480C430.1 480 448 462.1 448 440L448 256L416 256C363 256 320 213 320 160zM464 152C464 138.7 453.3 128 440 128C426.7 128 416 138.7 416 152C416 165.3 426.7 176 440 176C453.3 176 464 165.3 464 152z")
        };

        // Start loading versions
        _ = LoadVersionsAsync(MinecraftPluginData.AllInstallableMinecraftVersions.Count != 0);
    }

    private void BindingEvent()
    {
        // Wire up search text change events
        PropertyChanged += (sender, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(AllVersionsSearchText):
                    FilterVersions(_allVersions, _filteredAllVersions, AllVersionsSearchText);
                    break;
                case nameof(ReleaseVersionsSearchText):
                    FilterVersions(_releaseVersions, _filteredReleaseVersions, ReleaseVersionsSearchText);
                    break;
                case nameof(SnapshotVersionsSearchText):
                    FilterVersions(_snapshotVersions, _filteredSnapshotVersions, SnapshotVersionsSearchText);
                    break;
                case nameof(OldVersionsSearchText):
                    FilterVersions(_oldVersions, _filteredOldVersions, OldVersionsSearchText);
                    break;
            }
        };
    }

    public PageLoadingAnimator InAnimator { get; set; }
    private string _shortInfo = string.Empty;

    public string ShortInfo
    {
        get => _shortInfo;
        set => SetField(ref _shortInfo, value);
    }

    public Control BottomElement { get; set; }
    public Control RootElement { get; set; }

    // Loading and error state properties
    public bool IsLoading
    {
        get => _isLoading;
        set => SetField(ref _isLoading, value);
    }

    public bool HasError
    {
        get => _hasError;
        set => SetField(ref _hasError, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetField(ref _errorMessage, value);
    }

    public bool IsContentVisible => !IsLoading && !HasError;

    // Latest version properties
    public VersionManifestEntry LatestSnapshot
    {
        get => _latestSnapshot;
        set => SetField(ref _latestSnapshot, value);
    }

    public VersionManifestEntry LatestRelease
    {
        get => _latestRelease;
        set => SetField(ref _latestRelease, value);
    }

    // Search text properties
    public string AllVersionsSearchText
    {
        get => _allVersionsSearchText;
        set => SetField(ref _allVersionsSearchText, value);
    }

    public string ReleaseVersionsSearchText
    {
        get => _releaseVersionsSearchText;
        set => SetField(ref _releaseVersionsSearchText, value);
    }

    public string SnapshotVersionsSearchText
    {
        get => _snapshotVersionsSearchText;
        set => SetField(ref _snapshotVersionsSearchText, value);
    }

    public string OldVersionsSearchText
    {
        get => _oldVersionsSearchText;
        set => SetField(ref _oldVersionsSearchText, value);
    }

    // Filtered collections
    public ObservableCollection<VersionManifestEntry> FilteredAllVersions => _filteredAllVersions;
    public ObservableCollection<VersionManifestEntry> FilteredReleaseVersions => _filteredReleaseVersions;
    public ObservableCollection<VersionManifestEntry> FilteredSnapshotVersions => _filteredSnapshotVersions;
    public ObservableCollection<VersionManifestEntry> FilteredOldVersions => _filteredOldVersions;

    // Core methods
    private async Task LoadVersionsAsync(bool fromCache = false)
    {
        var cts = new CancellationTokenSource();

        try
        {
            IsLoading = true;
            HasError = false;
            OnPropertyChanged(nameof(IsContentVisible));

            var got = false;

            if (!fromCache)
            {
                MinecraftPluginData.AllInstallableMinecraftVersions.CollectionChanged += (sender, args) =>
                {
                    if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && !got)
                    {
                        got = true;
                        cts.Cancel();
                        return;
                    }
                };

                // Fetch versions using VanillaInstaller
                var versions = (await VanillaInstaller.EnumerableMinecraftAsync(cts.Token)).ToList();
                got = true;

                // Store in MinecraftPluginData
                MinecraftPluginData.AllInstallableMinecraftVersions.Clear();
                foreach (var version in versions)
                {
                    MinecraftPluginData.AllInstallableMinecraftVersions.Add(version);
                }

                // Categorize versions
                CategorizeVersions(versions);
            }
            else
            {
                CategorizeVersions(MinecraftPluginData.AllInstallableMinecraftVersions.ToList());
            }

            // Update latest versions
            UpdateLatestVersions();

            // Initialize filtered collections
            InitializeFilteredCollections();

            IsLoading = false;
            OnPropertyChanged(nameof(IsContentVisible));
        }
        catch (Exception ex)
        {
            if (cts.IsCancellationRequested)
            {
                CategorizeVersions(MinecraftPluginData.AllInstallableMinecraftVersions.ToList());
                UpdateLatestVersions();

                // Initialize filtered collections
                InitializeFilteredCollections();

                IsLoading = false;
                OnPropertyChanged(nameof(IsContentVisible));
                return;
            }

            IsLoading = false;
            HasError = true;
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(IsContentVisible));
        }
    }


    private void CategorizeVersions(List<VersionManifestEntry> versions)
    {
        _allVersions = versions.OrderByDescending(v => v.ReleaseTime).ToList();
        _releaseVersions = versions.Where(v => v.Type == "release").OrderByDescending(v => v.ReleaseTime).ToList();
        _snapshotVersions = versions.Where(v => v.Type == "snapshot").OrderByDescending(v => v.ReleaseTime).ToList();

        // Define "old" versions as releases older than 1 year
        var oneYearAgo = DateTime.Now.AddYears(-1);
        _oldVersions = versions.Where(v => v.Type == "release" && v.ReleaseTime < oneYearAgo)
            .OrderByDescending(v => v.ReleaseTime).ToList();
    }

    private void UpdateLatestVersions()
    {
        var latestSnapshot = _snapshotVersions.FirstOrDefault();
        var latestRelease = _releaseVersions.FirstOrDefault();

        LatestSnapshot = latestSnapshot ?? new() { Id = "No snapshots available", ReleaseTime = DateTime.MinValue };
        LatestRelease = latestRelease ?? new() { Id = "No releases available", ReleaseTime = DateTime.MinValue };

        // Update MinecraftPluginData latest versions
        MinecraftPluginData.LatestInstallableMinecraftVersions.Clear();
        MinecraftPluginData.LatestInstallableMinecraftVersions.Add(LatestSnapshot);
        MinecraftPluginData.LatestInstallableMinecraftVersions.Add(LatestRelease);
    }

    private void InitializeFilteredCollections()
    {
        FilterVersions(_allVersions, _filteredAllVersions, AllVersionsSearchText);
        FilterVersions(_releaseVersions, _filteredReleaseVersions, ReleaseVersionsSearchText);
        FilterVersions(_snapshotVersions, _filteredSnapshotVersions, SnapshotVersionsSearchText);
        FilterVersions(_oldVersions, _filteredOldVersions, OldVersionsSearchText);
    }

    private void FilterVersions(List<VersionManifestEntry> sourceVersions,
        ObservableCollection<VersionManifestEntry> targetCollection,
        string searchText)
    {
        targetCollection.Clear();

        var filteredVersions = string.IsNullOrWhiteSpace(searchText)
            ? sourceVersions
            : sourceVersions.Where(v => v.Id.Contains(searchText, StringComparison.OrdinalIgnoreCase));

        foreach (var version in filteredVersions)
        {
            targetCollection.Add(version);
        }
    }

    // Event handlers
    public async void OnRetryClick(object? sender, RoutedEventArgs e)
    {
        await LoadVersionsAsync();
    }

    public void OnVersionSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is VersionManifestEntry selectedVersion)
        {
            // Clear selection to allow re-selecting the same item
            listBox.SelectedItem = null;

            // Call installation method
            InstallVersion(selectedVersion);
        }
    }

    // Latest version card click handlers
    public void OnLatestSnapshotClick(object? sender, PointerPressedEventArgs e)
    {
        if (LatestSnapshot != null && !string.IsNullOrEmpty(LatestSnapshot.Id) && LatestSnapshot.Id != MainLang.Loading)
        {
            InstallVersion(LatestSnapshot);
        }
    }

    public void OnLatestReleaseClick(object? sender, PointerPressedEventArgs e)
    {
        if (LatestRelease != null && !string.IsNullOrEmpty(LatestRelease.Id) && LatestRelease.Id != MainLang.Loading)
        {
            InstallVersion(LatestRelease);
        }
    }

    // Placeholder installation method
    private void InstallVersion(VersionManifestEntry version)
    {
        var root = this.GetVisualRoot();
        if (root is TabWindow tabWindow)
        {
            tabWindow.CreateTab(new TabEntry(new InstallPreview(version)));
            HostTab.Close();
            return;
        }

        App.UiRoot.CreateTab(new TabEntry(new InstallPreview(version)));
        HostTab.Close();

    }

    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public void OnClose()
    {
    }

    public static AurelioStaticPageInfo StaticPageInfo { get; } = new()
    {
        Icon = StreamGeometry.Parse(
            "M320 160C320 107 363 64 416 64L454.4 64C503.9 64 544 104.1 544 153.6L544 440C544 515.1 483.1 576 408 576C332.9 576 272 515.1 272 440L272 360C272 337.9 254.1 320 232 320C209.9 320 192 337.9 192 360L192 528C192 554.5 170.5 576 144 576C117.5 576 96 554.5 96 528L96 360C96 284.9 156.9 224 232 224C307.1 224 368 284.9 368 360L368 440C368 462.1 385.9 480 408 480C430.1 480 448 462.1 448 440L448 256L416 256C363 256 320 213 320 160zM464 152C464 138.7 453.3 128 440 128C426.7 128 416 138.7 416 152C416 165.3 426.7 176 440 176C453.3 176 464 165.3 464 152z"),
        Title = MainLang.AutoInstall + " Minecraft",
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
}