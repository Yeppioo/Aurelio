using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Plugin.Base;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Classes.Setting;
using Aurelio.Views.Main.Pages;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Const;

public class Data : ReactiveObject
{
    private static Data? _instance;

    public static Data Instance
    {
        get { return _instance ??= new Data(); }
    }

    public static DesktopType DesktopType { get; set; } = DesktopType.Unknown;
    public static SettingEntry SettingEntry { get; set; }
    public static UiProperty UiProperty { get; set; } = UiProperty.Instance;
    public static string TranslateToken { get; set; }
    [Reactive] public string Version { get; set; }
    public static List<RecordMinecraftEntry> AllMinecraftInstances { get; } = [];
    public static ObservableCollection<MinecraftCategoryEntry> SortedMinecraftCategories { get; } = [];
    public static ObservableCollection<IPlugin> LoadedPlugins { get; } = [];
    public static ObservableCollection<AggregateSearchEntry> AggregateSearchEntries { get; } = [];

    public static void UpdateAggregateSearchEntries()
    {
        AggregateSearchEntries.Clear();
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            AggregateSearchEntries.Add(new AggregateSearchEntry(new NewTabPage(), null));
            AggregateSearchEntries.Add(new AggregateSearchEntry(new SettingTabPage() , "setting"));
            AggregateSearchEntries.Add(new AggregateSearchEntry(new MinecraftInstancesTabPage(), "minecraftInstances"));
        });
        AggregateSearchEntries.AddRange(AllMinecraftInstances.Select(x => new AggregateSearchEntry(x)));
        if (SettingEntry != null)
            AggregateSearchEntries.AddRange(SettingEntry.MinecraftAccounts.Select(x => new AggregateSearchEntry(x)));
    }
}