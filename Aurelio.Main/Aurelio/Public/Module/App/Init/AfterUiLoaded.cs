﻿using System.Diagnostics;
using System.IO;
using System.Linq;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Module.App.Services;
using Aurelio.Public.Module.Service.Minecraft;
using Aurelio.Public.Module.Ui;
using Avalonia.Media;
using AurelioPage = Aurelio.Views.Main.Pages.SubPages.SettingPages.AurelioPage;

namespace Aurelio.Public.Module.App.Init;

public abstract class AfterUiLoaded
{
    public static void Main()
    {
        File.WriteAllText(ConfigPath.AppPathDataPath,
            Process.GetCurrentProcess().MainModule.FileName);
        BindKeys.Main(Aurelio.App.UiRoot!);
        Data.SettingEntry.MinecraftAccounts.CollectionChanged += (_, _) => Data.UpdateAggregateSearchEntries();
        _ = MinecraftInstancesHandler.Load(Data.SettingEntry.MinecraftFolderEntries.Select(x => x.Path).ToArray());
        Setter.SetAccentColor(Data.SettingEntry.ThemeColor);
        Application.Current.Resources["BackGroundOpacity"] = Data.SettingEntry.BackGround == Setting.BackGround.Default ? 1.0 : 0.5;
        _ = TranslateToken.RefreshToken();
        Setter.ToggleTheme(Data.SettingEntry.Theme);
        LoopGC.BeginLoop();
        if (Data.SettingEntry.AutoCheckUpdate && Data.Instance.Version != "vDebug")
            _ = AurelioPage.ShowUpdateDialogIfNeed();
    }
}