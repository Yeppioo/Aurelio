using System.Linq;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.Operate;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;

namespace Aurelio.Views.Main.SubPages.SettingPages;

public partial class LaunchPage : PageMixModelBase, IAurelioPage
{
    public LaunchPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
    }

    public new static Data Data => Data.Instance;
    public PageLoadingAnimator OutAnimator { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }

    public Control RootElement { get; set; }

    private void BindingEvent()
    {
        AddMinecraftFolder.Click += async (_, _) => { await MinecraftFolder.AddByUi(this); };
        RemoveSelectedMinecraftFolder.Click += (_, _) =>
        {
            var item = MinecraftFolderListBox.SelectedItem;
            if (item is RecordMinecraftFolderEntry folder)
            {
                Data.SettingEntry.MinecraftFolderEntries.Remove(folder);
                MinecraftFolderListBox.SelectedItem = MinecraftFolderListBox.Items.FirstOrDefault();
            }

            AppMethod.SaveSetting();
        };
        AutoScanJavaRuntime.Click += (_, _) => { JavaRuntime.AddByAutoScan(); };
        AddJavaRuntime.Click += (_, _) => { _ = JavaRuntime.AddByUi(this); };
        RemoveSelectedJavaRuntime.Click += (_, _) =>
        {
            var item = JavaRuntimeComboBox.SelectedItem;
            if (item is RecordJavaRuntime runtime)
            {
                Data.SettingEntry.JavaRuntimes.Remove(runtime);
                JavaRuntimeComboBox.SelectedItem = JavaRuntimeComboBox.Items.FirstOrDefault();
            }

            AppMethod.SaveSetting();
        };
    }
}