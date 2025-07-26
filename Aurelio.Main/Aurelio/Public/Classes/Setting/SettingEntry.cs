using System.ComponentModel;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.Ui;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Setting;

public class SettingEntry : ReactiveObject
{
    public SettingEntry()
    {
        PropertyChanged += OnPropertyChanged;
    }

    [Reactive] [JsonProperty] public Enum.Setting.NoticeWay NoticeWay { get; set; } = Enum.Setting.NoticeWay.Bubble;
    [Reactive] [JsonProperty] public Enum.Setting.Theme Theme { get; set; } = Enum.Setting.Theme.Dark;
    [Reactive] [JsonProperty] public Enum.Setting.BackGround BackGround { get; set; } = Enum.Setting.BackGround.Default;

    [Reactive]
    [JsonProperty]
    public Enum.Setting.LaunchPage LaunchPage { get; set; } = Enum.Setting.LaunchPage.MinecraftInstance;

    [Reactive] [JsonProperty] public Color ThemeColor { get; set; } = Color.Parse("#1BD76A");
    [Reactive] [JsonProperty] public Color BackGroundColor { get; set; } = Color.Parse("#00B7FF52");
    [Reactive] [JsonProperty] public Language Language { get; set; } = LanguageTypes.Langs[0];
    [Reactive] [JsonProperty] public bool UseFilePicker { get; set; } = true;
    [Reactive] [JsonProperty] public bool AutoCheckUpdate { get; set; } = true;
    [Reactive] [JsonProperty] public bool EnableSpeedUpGithubApi { get; set; } = true;
    [Reactive] [JsonProperty] public string PoemApiToken { get; set; }
    [Reactive] [JsonProperty] public string GithubSpeedUpApiUrl { get; set; } = "https://ghproxy.net/%url%";
    [Reactive] [JsonProperty] public string BackGroundImgData { get; set; }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Theme))
        {
            Setter.ToggleTheme(Theme);
        }
        else if (e.PropertyName == nameof(ThemeColor))
        {
            Setter.SetAccentColor(ThemeColor);
        }
        else if (e.PropertyName == nameof(Language))
        {
            GlobalLangHelper.Current.ChangedCulture(Language.Code == "zh-CN" ? "" : Language.Code);
            Notice(MainLang.NeedRestartApp, NotificationType.Warning);
        }
        else if (e.PropertyName == nameof(BackGround))
        {
            Application.Current.Resources["BackGroundOpacity"] =
                BackGround == Enum.Setting.BackGround.Default ? 1.0 : 0.5;
        }
        else if (e.PropertyName == nameof(NoticeWay))
        {
            Notice(MainLang.ExampleNotification);
        }


        AppMethod.SaveSetting();
    }
}