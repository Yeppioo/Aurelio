using System;
using System.Linq;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App;
using Aurelio.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aurelio.Views.Main.SettingPages;

public partial class LaunchPage : PageMixModelBase
{
    public LaunchPage()
    {
        InitializeComponent();
        ListBox.DataContext = Data.Instance;
        DataContext = this;
        BindingEvent();
    }
    private void BindingEvent()
    {
        AddMinecraftFolder.Click += async (_, _) =>
        {
            await Public.Module.Op.MinecraftFolder.AddByUi(this);
            MinecraftFolderExpander.MaxHeight = Data.SettingEntry.MinecraftFolderEntries.Count * 48 + 25;
        };
        RemoveSelectedMinecraftFolder.Click += (_, _) =>
        {
            var item = ListBox.SelectedItem;
            if (item is MinecraftFolderEntry folder)
            {
                Data.SettingEntry.MinecraftFolderEntries.Remove(folder);
                ListBox.SelectedItem = ListBox.Items.FirstOrDefault();
            }
            AppMethod.SaveSetting();
            MinecraftFolderExpander.MaxHeight = Data.SettingEntry.MinecraftFolderEntries.Count * 48 + 25;
        };
        Loaded += (_, _) =>
        {
            MinecraftFolderExpander.MaxHeight = Data.SettingEntry.MinecraftFolderEntries.Count * 48 + 25;
        };
    }
}