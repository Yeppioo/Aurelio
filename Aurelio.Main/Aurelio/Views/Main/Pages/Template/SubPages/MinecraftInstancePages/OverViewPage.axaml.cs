using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Public.Module;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia.Controls.Notifications;
using FluentAvalonia.UI.Controls;
using Aurelio.Public.Const;

namespace Aurelio.Views.Main.Pages.Template.SubPages.MinecraftInstancePages;

public partial class OverViewPage : PageMixModelBase, IAurelioPage
{
    public OverViewPage(RecordMinecraftEntry entry)
    {
        InitializeComponent();
        Entry = entry;
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));

        // 标签集合变化时自动清理重复项
        Entry.SettingEntry.Tags.CollectionChanged += (_, _) => Entry.SettingEntry.RemoveDuplicateTags();

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

        // 监控标签选择变化，自动去除重复
        TagsMultiComboBox.SelectionChanged += (_, _) =>
        {
            Entry.SettingEntry.RemoveDuplicateTags();
        };
    }

    public async void EditMinecraftIdCommand(Control sender)
    {
        var text = new TextBox { Text = Entry.Id };
        var d = await ShowDialogAsync(MainLang.Rename, p_content: text, b_primary: MainLang.Ok,
            b_cancel: MainLang.Cancel, sender: sender);
        if (d != ContentDialogResult.Primary) return;
    }

    public async void CreateNewTagCommand(Control sender)
    {
        var text = new TextBox { Watermark = MainLang.Name };
        var d = await ShowDialogAsync(MainLang.New, p_content: text, b_primary: MainLang.Ok,
            b_cancel: MainLang.Cancel, sender: sender);
        if (d != ContentDialogResult.Primary || text.Text.IsNullOrWhiteSpace()) return;

        var newTag = text.Text!.Trim();

        // 验证标签名称
        if (!Entry.SettingEntry.IsValidTagName(newTag))
        {
            Notice(MainLang.IncludeSpecialWord, NotificationType.Error);
            return;
        }

        // 检查是否已存在
        if (Entry.SettingEntry.Tags.Contains(newTag))
        {
            Notice($"{newTag} - {MainLang.TheItemAlreadyExist}", NotificationType.Warning);
            return;
        }

        // 添加到全局标签列表和当前实例
        if (!UiProperty.AllMinecraftTags.Contains(newTag))
            UiProperty.AllMinecraftTags.Add(newTag);

        Entry.SettingEntry.AddTag(newTag);
    }

    /// <summary>
    /// 切换收藏状态
    /// </summary>
    public void ToggleFavouriteCommand(Control sender)
    {
        Entry.SettingEntry.IsFavourite = !Entry.SettingEntry.IsFavourite;
    }
}