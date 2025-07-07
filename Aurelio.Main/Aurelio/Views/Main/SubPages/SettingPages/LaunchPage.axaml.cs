using System.Linq;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;

namespace Aurelio.Views.Main.SubPages.SettingPages;

public partial class LaunchPage : PageMixModelBase, IAurelioPage
{
    public static Data Data => Data.Instance;
    public PageLoadingAnimator InAnimator { get; set; }
    public PageLoadingAnimator OutAnimator { get; set; }

    public LaunchPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
    }

    private void BindingEvent()
    {
        AddMinecraftFolder.Click += async (_, _) => { await Public.Module.Op.MinecraftFolder.AddByUi(this); };
        RemoveSelectedMinecraftFolder.Click += (_, _) =>
        {
            var item = MinecraftFolderListBox.SelectedItem;
            if (item is MinecraftFolderEntry folder)
            {
                Data.SettingEntry.MinecraftFolderEntries.Remove(folder);
                MinecraftFolderListBox.SelectedItem = MinecraftFolderListBox.Items.FirstOrDefault();
            }

            AppMethod.SaveSetting();
        };
        AutoScanJavaRuntime.Click += (_, _) => { Public.Module.Op.JavaRuntime.AddByAutoScan(); };
        AddJavaRuntime.Click += (_, _) => { _ = Public.Module.Op.JavaRuntime.AddByUi(this); };
        RemoveSelectedJavaRuntime.Click += (_, _) =>
        {
            var item = JavaRuntimeListBox.SelectedItem;
            if (item is RecordJavaRuntime runtime)
            {
                Data.SettingEntry.JavaRuntimes.Remove(runtime);
                JavaRuntimeListBox.SelectedItem = JavaRuntimeListBox.Items.FirstOrDefault();
            }

            AppMethod.SaveSetting();
        };
    }

    public Border RootElement { get; set; }
}