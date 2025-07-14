using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Public.Module;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Avalonia.Controls.Notifications;
using FluentAvalonia.UI.Controls;

namespace Aurelio.Views.Main.Template.SubPages.MinecraftInstancePages;

public partial class OverViewPage : PageMixModelBase, IAurelioPage
{
    public OverViewPage(RecordMinecraftEntry entry)
    {
        InitializeComponent();
        Entry = entry;
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
    }

    public OverViewPage()
    {
    }

    public RecordMinecraftEntry Entry { get; }
    public ObservableCollection<RecordJavaRuntime> JavaRuntimes { get; } = [];

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }

    private void BindingEvent()
    {
        Loaded += (_, _) =>
        {
            // 先清理掉可能的重复标签
            Entry.SettingEntry.RemoveDuplicateTags();

            JavaRuntimes.Clear();
            JavaRuntimes.Add(new RecordJavaRuntime
            {
                JavaVersion = "global",
                JavaPath = MainLang.UseGlobalSetting
            });
            foreach (var item in Data.SettingEntry.JavaRuntimes) JavaRuntimes.Add(item);

            var matchingRuntime = JavaRuntimes.FirstOrDefault(j => j == Entry.SettingEntry.JavaRuntime);
            JavaRuntimeComboBox.SelectedItem = matchingRuntime ?? JavaRuntimes[0];
        };
        JavaRuntimeComboBox.SelectionChanged += (_, _) =>
        {
            Entry.SettingEntry.JavaRuntime = JavaRuntimeComboBox.SelectedItem as RecordJavaRuntime;
        };

        // 监控标签选择变化
        TagsMultiComboBox.SelectionChanged += (_, _) =>
        {
            // 每次选择变化后，自动去除重复
            Entry.SettingEntry.RemoveDuplicateTags();
        };
    }

    public async void EditMinecraftIdCommand()
    {
        var text = new TextBox { Text = Entry.Id };
        var d = await ShowDialogAsync(MainLang.Rename, p_content: text, b_primary: MainLang.Ok,
            b_cancel: MainLang.Cancel);
        if (d != ContentDialogResult.Primary) return;
    }

    public async void CreateNewTagCommand()
    {
        var text = new TextBox { Watermark = MainLang.Name };
        var d = await ShowDialogAsync(MainLang.New, p_content: text, b_primary: MainLang.Ok,
            b_cancel: MainLang.Cancel);
        if (d != ContentDialogResult.Primary || text.Text.IsNullOrWhiteSpace()) return;

        string newTag = text.Text!;

        // 检查是否为内置标签名
        if (UiProperty.BuiltInTags.Contains(newTag))
        {
            // 显示错误提示，不允许创建与内置标签同名的标签
            UiProperty.Toast.Show($"{newTag} - 这是预留的标签名，无法创建", NotificationType.Error);
            return;
        }

        // 检查标签是否已存在
        if (!UiProperty.AllMinecraftTags.Contains(newTag))
        {
            UiProperty.AllMinecraftTags.Add(newTag);
        }
    }
}