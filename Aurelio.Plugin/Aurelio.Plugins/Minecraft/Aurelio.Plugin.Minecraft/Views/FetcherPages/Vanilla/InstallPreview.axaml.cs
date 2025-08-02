using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Aurelio.Views.Main;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Installer;

namespace Aurelio.Plugin.Minecraft.Views.FetcherPages.Vanilla;

public partial class InstallPreview : PageMixModelBase, IAurelioTabPage, IAurelioNavPage
{
    private readonly VersionManifestEntry _entry;

    public InstallPreview()
    {
    }

    public InstallPreview(VersionManifestEntry entry)
    {
        _entry = entry;
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        ShortInfo = $"{MainLang.InstallPreview} / {entry.Id}";
        PageInfo = new PageInfoEntry
        {
            Title = MainLang.AutoInstall,
            Icon = StreamGeometry.Parse(
                "M320 160C320 107 363 64 416 64L454.4 64C503.9 64 544 104.1 544 153.6L544 440C544 515.1 483.1 576 408 576C332.9 576 272 515.1 272 440L272 360C272 337.9 254.1 320 232 320C209.9 320 192 337.9 192 360L192 528C192 554.5 170.5 576 144 576C117.5 576 96 554.5 96 528L96 360C96 284.9 156.9 224 232 224C307.1 224 368 284.9 368 360L368 440C368 462.1 385.9 480 408 480C430.1 480 448 462.1 448 440L448 256L416 256C363 256 320 213 320 160zM464 152C464 138.7 453.3 128 440 128C426.7 128 416 138.7 416 152C416 165.3 426.7 176 440 176C453.3 176 464 165.3 464 152z")
        };
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

    public async Task Test(string[] args)
    {
        await OptifineInstaller.EnumerableOptifineAsync(_entry.Id);
        await FabricInstaller.EnumerableFabricAsync(_entry.Id);
        await QuiltInstaller.EnumerableQuiltAsync(_entry.Id);
        await ForgeInstaller.EnumerableForgeAsync(_entry.Id , true); //NeoForge
        await ForgeInstaller.EnumerableForgeAsync(_entry.Id); //Forge
        
    }
    
    
}
