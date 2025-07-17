using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;

namespace Aurelio.Views.Main.Pages.Instance.SubPages.SettingPages;

public partial class AurelioPage : PageMixModelBase, IAurelioPage
{
    public AurelioPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public static async Task ShowUpdateDialogIfNeed()
    {
        var info = await Public.Module.App.Services.Update.CheckUpdate();
        if (info.IsNeedUpdate)
        {
            var changelogPanel = ParseMarkdown(info.Body);
            var c = new StackPanel()
            {
                Spacing = 10,
                Children =
                {
                    new SelectableTextBlock
                        { Text = "https://github.com/Yeppioo/Aurelio/releases/tag/auto-publish" },
                    changelogPanel
                }
            };
            if (Data.DesktopType == DesktopType.Windows && Environment.OSVersion.Version.Major >= 10)
            {
                var cr = await ShowDialogAsync($"{MainLang.FoundNewVersion}: {info.NewVersion}", p_content: c, b_primary: MainLang.Update,
                    b_secondary: MainLang.SaveAs, b_cancel: MainLang.Cancel, sender: UiProperty.ActiveWindow);
                if (cr == ContentDialogResult.Primary)
                {
                    _ = Public.Module.App.Services.Update.UpdateApp(UiProperty.ActiveWindow);
                }
                else if (cr == ContentDialogResult.Secondary)
                {
                    SaveAs(UiProperty.ActiveWindow);
                }
            }
            else
            {
                var cr = await ShowDialogAsync($"{MainLang.FoundNewVersion}: {info.NewVersion}", p_content: c, b_primary: MainLang.SaveAs,
                    b_cancel: MainLang.Cancel, sender: UiProperty.ActiveWindow, b_secondary: MainLang.CloseAutoUpdate);
                if (cr == ContentDialogResult.Primary)
                {
                    SaveAs(UiProperty.ActiveWindow);
                }
                else if (cr == ContentDialogResult.Secondary)
                {
                    Data.SettingEntry.AutoCheckUpdate = false;
                }
            }
        }
    }
    
    private async void Update_OnClick(object? sender, RoutedEventArgs e)
    {
        Update.IsLoading = true;
        Update.IsEnabled = false;
        Update.Content = MainLang.CheckingUpdate;
        try
        {
            var info = await Public.Module.App.Services.Update.CheckUpdate();
            Update.IsLoading = false;
            Update.IsEnabled = true;
            Update.Content = MainLang.CheckUpdate;
            if (info.IsNeedUpdate)
            {
                var changelogPanel = ParseMarkdown(info.Body);
                var c = new StackPanel()
                {
                    Spacing = 10,
                    Children =
                    {
                        new SelectableTextBlock
                            { Text = "https://github.com/Yeppioo/Aurelio/releases/tag/auto-publish" },
                        changelogPanel
                    }
                };
                if (Data.DesktopType == DesktopType.Windows && Environment.OSVersion.Version.Major >= 10)
                {
                    var cr = await ShowDialogAsync($"{MainLang.FoundNewVersion}: {info.NewVersion}", p_content: c, b_primary: MainLang.Update,
                        b_secondary: MainLang.SaveAs, b_cancel: MainLang.Cancel, sender: sender as Control);
                    if (cr == ContentDialogResult.Primary)
                    {
                        _ = Public.Module.App.Services.Update.UpdateApp(this);
                    }
                    else if (cr == ContentDialogResult.Secondary)
                    {
                        SaveAs(this);
                    }
                }
                else
                {
                    var cr = await ShowDialogAsync($"{MainLang.FoundNewVersion}: {info.NewVersion}", p_content: c, b_primary: MainLang.SaveAs,
                        b_cancel: MainLang.Cancel, sender: sender as Control);
                    if (cr == ContentDialogResult.Primary)
                    {
                        SaveAs(this);
                    }
                }
            }
            else
            {
                Notice($"{MainLang.LatestVersion}" , NotificationType.Success);
            }
        }
        catch (Exception exception)
        {
            Logger.Error(exception);
            Notice(MainLang.CheckUpdateFail, NotificationType.Error);
        }

        Update.IsLoading = false;
        Update.IsEnabled = true;
        Update.Content = MainLang.CheckUpdate;
    }

    private static async void SaveAs(Control sender)
    {
        var fileList = new List<DownloadFileInfo>
        {
            new("Windows x64 便携版", "Aurelio.win.x64.executable.zip"),
            new("Windows x64 安装程序", "Aurelio.win.x64.installer.exe"),
            new("Windows x86 便携版", "Aurelio.win.x86.executable.zip"),
            new("Windows x86 安装程序", "Aurelio.win.x86.installer.exe"),
            new("Windows ARM64 便携版", "Aurelio.win.arm64.executable.zip"),
            new("Windows ARM64 安装程序", "Aurelio.win.arm64.installer.exe"),
            new("macOS ARM64 应用程序包", "Aurelio.osx.mac.arm64.app.zip"),
            new("macOS ARM64 安装程序", "Aurelio.osx.mac.arm64.dmg"),
            new("macOS x64 应用程序包", "Aurelio.osx.mac.x64.app.zip"),
            new("macOS x64 安装程序", "Aurelio.osx.mac.x64.dmg"),
            new("Linux ARM AppImage", "Aurelio.linux.arm.AppImage"),
            new("Linux ARM64 AppImage", "Aurelio.linux.arm64.AppImage"),
            new("Linux x64 AppImage", "Aurelio.linux.x64.AppImage"),
        };

        var listBox = new ListBox
        {
            ItemsSource = fileList, Width = 420,
            ItemTemplate = new FuncDataTemplate<DownloadFileInfo>((item, _) =>
            {
                if (item == null) return new TextBlock { Text = "Error" };

                var stackPanel = new StackPanel
                {
                    Margin = new Thickness(8, 6, 8, 6)
                };

                // 主标题
                var titleBlock = new TextBlock
                {
                    Text = item.Description,
                    FontSize = 14,
                    FontWeight = FontWeight.Normal
                };

                // 副标题（文件名）
                var fileNameBlock = new TextBlock
                {
                    Text = item.FileName,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.Parse("#7e7e7e")),
                    Margin = new Thickness(0, 2, 0, 0)
                };

                stackPanel.Children.Add(titleBlock);
                stackPanel.Children.Add(fileNameBlock);

                return stackPanel;
            })
        };

        listBox.SelectedIndex = 0;

        var dialog = await ShowDialogAsync(
            title: MainLang.SaveAs,
            p_content: listBox,
            b_primary: MainLang.Download,
            b_cancel: MainLang.Cancel,
            sender: sender
        );

        if (dialog == ContentDialogResult.Primary && listBox.SelectedItem is DownloadFileInfo selectedFile)
        {
            var path = (await TopLevel.GetTopLevel(sender).StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    Title = MainLang.SaveAs,
                    SuggestedFileName = selectedFile.FileName,
                    FileTypeChoices =
                    [
                        new FilePickerFileType("App File")
                            { Patterns = [$"*{Path.GetExtension(selectedFile.FileName)}"] }
                    ]
                }))?.Path.LocalPath;
            if (string.IsNullOrWhiteSpace(path)) return;
            Notice($"{MainLang.BeginDownload}: {selectedFile.FileName}");
            await Public.Module.App.Services.Update.Download(selectedFile.FileName, path, sender);
        }
    }

    // 文件信息数据模型
    private class DownloadFileInfo
    {
        public string Description { get; }
        public string FileName { get; }

        public DownloadFileInfo(string description, string fileName)
        {
            Description = description;
            FileName = fileName;
        }
    }

    private static Panel ParseMarkdown(string markdown)
    {
        var panel = new StackPanel { Spacing = 8 };

        if (string.IsNullOrWhiteSpace(markdown))
            return panel;

        var lines = markdown.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine))
                continue;

            // 处理标题 (## Title)
            if (trimmedLine.StartsWith("## "))
            {
                var headerText = trimmedLine[3..].Trim();
                var headerBlock = new TextBlock
                {
                    Text = headerText,
                    FontSize = 16,
                    FontWeight = FontWeight.Bold,
                    Margin = new Thickness(0, 8, 0, 4)
                };
                panel.Children.Add(headerBlock);
            }
            // 处理列表项 (- Item)
            else if (trimmedLine.StartsWith("- "))
            {
                var itemText = trimmedLine[2..].Trim();
                var itemPanel = ParseListItem(itemText);
                panel.Children.Add(itemPanel);
            }
        }

        return panel;
    }

    private static Panel ParseListItem(string itemText)
    {
        var itemPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(16, 2, 0, 2)
        };

        // 添加列表项前缀
        var bulletBlock = new TextBlock
        {
            Text = "• ",
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 0, 4, 0)
        };
        itemPanel.Children.Add(bulletBlock);

        // 解析文本中的链接和普通文本
        var contentPanel = new StackPanel { Orientation = Orientation.Horizontal };
        ParseInlineContent(itemText, contentPanel);
        itemPanel.Children.Add(contentPanel);

        return itemPanel;
    }

    private static void ParseInlineContent(string text, Panel container)
    {
        // 使用正则表达式匹配 Markdown 链接格式 [text](url) 和粗体格式 **text**
        var linkPattern = @"\[([^\]]+)\]\(([^)]+)\)";
        var boldPattern = @"\*\*([^*]+)\*\*";

        // 合并所有匹配项并按位置排序
        var allMatches = new List<(Match match, string type)>();

        foreach (Match match in Regex.Matches(text, linkPattern))
        {
            allMatches.Add((match, "link"));
        }

        foreach (Match match in Regex.Matches(text, boldPattern))
        {
            allMatches.Add((match, "bold"));
        }

        // 按位置排序
        allMatches.Sort((a, b) => a.match.Index.CompareTo(b.match.Index));

        if (allMatches.Count == 0)
        {
            // 没有特殊格式，直接添加文本
            var textBlock = new SelectableTextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap
            };
            container.Children.Add(textBlock);
            return;
        }

        int lastIndex = 0;

        foreach (var (match, type) in allMatches)
        {
            // 添加匹配前的文本
            if (match.Index > lastIndex)
            {
                var beforeText = text[lastIndex..match.Index];
                if (!string.IsNullOrEmpty(beforeText))
                {
                    var beforeBlock = new SelectableTextBlock
                    {
                        Text = beforeText,
                        TextWrapping = TextWrapping.Wrap
                    };
                    container.Children.Add(beforeBlock);
                }
            }

            if (type == "link")
            {
                // 创建超链接按钮
                var linkText = match.Groups[1].Value;
                var linkUrl = match.Groups[2].Value;

                var hyperlinkButton = new HyperlinkButton
                {
                    Content = linkText,
                    NavigateUri = null, // 禁用默认导航行为
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                // 处理链接点击事件
                hyperlinkButton.Click += async (_, e) =>
                {
                    e.Handled = true; // 阻止事件冒泡
                    try
                    {
                        var launcher = TopLevel.GetTopLevel(UiProperty.ActiveWindow)?.Launcher;
                        if (launcher != null)
                        {
                            await launcher.LaunchUriAsync(new Uri(linkUrl));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        Notice($"无法打开链接: {linkUrl}", NotificationType.Error);
                    }
                };

                container.Children.Add(hyperlinkButton);
            }
            else if (type == "bold")
            {
                // 创建粗体文本
                var boldText = match.Groups[1].Value;
                var boldBlock = new SelectableTextBlock
                {
                    Text = boldText,
                    FontWeight = FontWeight.Bold,
                    TextWrapping = TextWrapping.Wrap
                };
                container.Children.Add(boldBlock);
            }

            lastIndex = match.Index + match.Length;
        }

        // 添加最后一个匹配后的文本
        if (lastIndex < text.Length)
        {
            var afterText = text[lastIndex..];
            if (!string.IsNullOrEmpty(afterText))
            {
                var afterBlock = new SelectableTextBlock
                {
                    Text = afterText,
                    TextWrapping = TextWrapping.Wrap
                };
                container.Children.Add(afterBlock);
            }
        }
    }
}