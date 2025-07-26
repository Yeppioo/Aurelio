using System.Collections.ObjectModel;
using Aurelio.Plugin.Base;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Setting;
using Aurelio.Public.Module.Plugin.Events;
using Aurelio.Views.Main.Pages;
using Avalonia.Threading;
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
    public static ObservableCollection<IPlugin> LoadedPlugins { get; } = [];
    public static ObservableCollection<AggregateSearchEntry> AggregateSearchEntries { get; } = [];

    public static void UpdateAggregateSearchEntries()
    {
        AggregateSearchEntries.Clear();
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            AggregateSearchEntries.Add(new AggregateSearchEntry(new NewTabPage(), null));
            AggregateSearchEntries.Add(new AggregateSearchEntry(new SettingTabPage() , "setting"));
        });
        AppEvents.OnUpdateAggregateSearchEntries();
    }

    public Data()
    {
        PublicEvents.UpdateAggregateSearchEntries += (_,_) => UpdateAggregateSearchEntries();
    }
}