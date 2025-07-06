using System;
using System.Linq;
using System.Threading;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SukiUI;
using SukiUI.Helpers;

namespace Aurelio.Views.Main.SettingPages;

public partial class LaunchPage : PageMixModelBase , IAurelioPage
{
    public PageLoadingAnimator InAnimator { get; set; }

    public LaunchPage()
    {
        InitializeComponent();
        ListBox.DataContext = Data.Instance;
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0,40,0,0), (0,1));
        BindingEvent();
    }
    
    private void BindingEvent()
    {
        AddMinecraftFolder.Click += async (_, _) => { await Public.Module.Op.MinecraftFolder.AddByUi(this); };
        RemoveSelectedMinecraftFolder.Click += (_, _) =>
        {
            var item = ListBox.SelectedItem;
            if (item is MinecraftFolderEntry folder)
            {
                Data.SettingEntry.MinecraftFolderEntries.Remove(folder);
                ListBox.SelectedItem = ListBox.Items.FirstOrDefault();
            }

            AppMethod.SaveSetting();
        };
    }

    public Border RootElement { get; set; }
}