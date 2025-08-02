using Aurelio.Plugin.Minecraft.Classes.Minecraft;
using Aurelio.Plugin.Minecraft.Operate;
using Aurelio.Plugin.Minecraft.Service.Minecraft;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia;
using Avalonia.Controls;

namespace Aurelio.Plugin.Minecraft.Views.SettingPages;

public partial class LaunchPage : PageMixModelBase, IAurelioPage
{
    public new MinecraftPluginData Data => MinecraftPluginData.Instance;

    public LaunchPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
        ShortInfo = $"{MainLang.Setting} / {MainLang.Launch}";
    }
    
    public PageLoadingAnimator InAnimator { get; set; }
    private string _shortInfo = string.Empty;

    public string ShortInfo
    {
        get => _shortInfo;
        set => SetField(ref _shortInfo, value);
    }
    public Control RootElement { get; set; }

    private void BindingEvent()
    {
        AddMinecraftFolder.Click += async (_, _) => { await MinecraftFolder.AddByUi(this); };
        RemoveSelectedMinecraftFolder.Click += (_, _) =>
        {
            var item = MinecraftFolderListBox.SelectedItem;
            if (item is RecordMinecraftFolderEntry folder)
            {
                MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftFolderEntries.Remove(folder);
                MinecraftFolderListBox.SelectedItem = Enumerable.FirstOrDefault<object?>(MinecraftFolderListBox.Items);
                _ = MinecraftInstancesHandler.Load(MinecraftPluginData.MinecraftPluginSettingEntry
                    .MinecraftFolderEntries.Select(x => x.Path).ToArray());
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
                MinecraftPluginData.MinecraftPluginSettingEntry.JavaRuntimes.Remove(runtime);
                JavaRuntimeComboBox.SelectedItem = JavaRuntimeComboBox.Items.FirstOrDefault();
            }

            AppMethod.SaveSetting();
        };
    }
}