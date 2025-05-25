using System.Collections.ObjectModel;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Langs;
using Aurelio.Public.Module;
using Aurelio.Public.Module.App;
using Aurelio.Views.Main;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Setting;

public class SettingEntry : ReactiveObject
{
    [Reactive] [JsonProperty] public ObservableCollection<AccountRecordEntry> AccountRecordEntries { get; set; } = [];
    [Reactive] [JsonProperty] public AccountRecordEntry? CurrentAccount { get; set; }
    [Reactive] [JsonProperty] public Enum.Setting.NoticeWay NoticeWay { get; set; } = Enum.Setting.NoticeWay.Bubble;

    public SettingEntry()
    {
        PropertyChanged += (_, _) =>
        { 
            AppMethod.SaveSetting();
        };
        AccountRecordEntries.CollectionChanged += (_, _) =>
        {
            AppMethod.SaveSetting();
        };
    }
}