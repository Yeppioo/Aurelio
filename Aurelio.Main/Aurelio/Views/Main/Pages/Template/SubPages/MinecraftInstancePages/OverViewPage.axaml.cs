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

        // 监听标签集合变化，同步更新显示标签
        Entry.SettingEntry.Tags.CollectionChanged += (_, _) => UpdateDisplayTags();

        BindingEvent();
    }

    public OverViewPage()
    {
    }

    public RecordMinecraftEntry Entry { get; }
    public ObservableCollection<RecordJavaRuntime> JavaRuntimes { get; } = [];

    // 用于显示的标签集合，将固定标识符转换为显示名称
    public ObservableCollection<string> DisplayTags { get; } = [];

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

            // 初始化显示标签
            UpdateDisplayTags();
        };
        JavaRuntimeComboBox.SelectionChanged += (_, _) =>
        {
            Entry.SettingEntry.JavaRuntime = JavaRuntimeComboBox.SelectedItem as RecordJavaRuntime;
        };

        // 监控标签选择变化
        TagsMultiComboBox.SelectionChanged += (_, _) =>
        {
            // 处理收藏夹标签的转换
            HandleFavouriteTagSelection();
            // 每次选择变化后，自动去除重复
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

        var newTag = text.Text!;

        if (UiProperty.BuiltInTags.Contains(newTag))
        {
            Notice($"{newTag} - {MainLang.ReservedTagNameTip}", NotificationType.Error);
            return;
        }

        if (!UiProperty.AllMinecraftTags.Contains(newTag)) UiProperty.AllMinecraftTags.Add(newTag);
    }

    /// <summary>
    /// 处理收藏夹标签的选择转换
    /// </summary>
    private void HandleFavouriteTagSelection()
    {
        var selectedTags = TagsMultiComboBox.SelectedItems?.Cast<string>().ToList() ?? new List<string>();

        // 检查是否选择了收藏夹显示名称
        bool shouldBeFavourite = selectedTags.Contains(UiProperty.FavouriteDisplayName);

        // 移除显示名称，添加或移除实际的固定标识符
        if (selectedTags.Contains(UiProperty.FavouriteDisplayName))
        {
            selectedTags.Remove(UiProperty.FavouriteDisplayName);
        }

        // 处理收藏夹标签
        if (shouldBeFavourite)
        {
            if (!selectedTags.Contains(UiProperty.FavouriteTag))
            {
                selectedTags.Add(UiProperty.FavouriteTag);
            }
        }
        else
        {
            selectedTags.Remove(UiProperty.FavouriteTag);
        }

        // 更新实际的标签集合
        Entry.SettingEntry.Tags.Clear();
        foreach (var tag in selectedTags)
        {
            Entry.SettingEntry.Tags.Add(tag);
        }

        // 更新显示标签
        UpdateDisplayTags();
    }

    /// <summary>
    /// 更新显示标签集合，将固定标识符转换为显示名称
    /// </summary>
    private void UpdateDisplayTags()
    {
        DisplayTags.Clear();

        foreach (var tag in Entry.SettingEntry.Tags)
        {
            if (tag == UiProperty.FavouriteTag)
            {
                // 将固定标识符转换为本地化显示名称
                DisplayTags.Add(UiProperty.FavouriteDisplayName);
            }
            else
            {
                DisplayTags.Add(tag);
            }
        }
    }
}