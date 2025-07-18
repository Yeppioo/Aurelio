using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO;
using Aurelio.Views.Main;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using Ursa.Common;
using Ursa.Controls;
using Ursa.Controls.Options;
using Notification = Ursa.Controls.Notification;
using TaskCenterDrawer = Aurelio.Views.Overlay.TaskCenterDrawer;

namespace Aurelio.Public.Module.Ui;

public abstract class Overlay
{
    public static async Task<ContentDialogResult> ShowDialogAsync(string title = "Title", string msg = null,
        Control? p_content = null, string b_primary = null, string b_cancel = null, string b_secondary = null,
        TopLevel? p_host = null, Control? sender = null)
    {
        var content = p_content ?? new SelectableTextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Text = msg
        };
        if (!string.IsNullOrWhiteSpace(msg) && p_content != null)
            content = new StackPanel
            {
                Spacing = 15,
                Children =
                {
                    new SelectableTextBlock
                    {
                        TextWrapping = TextWrapping.Wrap,
                        Text = msg
                    },
                    content
                }
            };

        if (string.IsNullOrWhiteSpace(msg) && p_content == null) content = null;

        var dialog = new ContentDialog
        {
            PrimaryButtonText = b_primary,
            Content = content,
            DefaultButton = ContentDialogButton.Primary,
            CloseButtonText = b_cancel,
            SecondaryButtonText = b_secondary,
            Title = title
        };
        var result = await dialog.ShowAsync(p_host ?? TopLevel.GetTopLevel(sender ?? Aurelio.App.UiRoot));
        return result;
    }

    public static void Notice(string msg, NotificationType type = NotificationType.Information, TimeSpan? time = null,
        Action? onClick = null, bool showTime = true, string title = "Aurelio", IAurelioWindow? host = null)
    {
        Logger.Info($"[Notice] [{type}] {msg}");
        var showTitle = "Aurelio";
        if (!string.IsNullOrWhiteSpace(title)) showTitle = title;
        if (showTime) showTitle += $" - {DateTime.Now:HH:mm:ss}";

        var notification = new Notification(showTitle, msg, type);
        UiProperty.Notifications.Insert(0, new NotificationEntry(notification, notification.Type));

        switch (Data.SettingEntry.NoticeWay)
        {
            case Setting.NoticeWay.Bubble:
                NotificationBubble(msg, type, time, onClick, host);
                break;
            case Setting.NoticeWay.Card:
                NotificationCard(msg, type, showTitle, time, onClick, host);
                break;
        }
    }

    public static void NotificationBubble(string msg, NotificationType type, TimeSpan? time = null,
        Action? onClick = null, IAurelioWindow? host = null)
    {
        var toast = new Toast(msg, type);
        (host != null ? host.Toast : UiProperty.Toast).Show(toast, toast.Type, classes: ["Light"], onClick: onClick,
            expiration: time ?? TimeSpan.FromSeconds(3.0));
    }

    public static void NotificationCard(string msg, NotificationType type, string title, TimeSpan? time = null,
        Action? onClick = null, IAurelioWindow? host = null)
    {
        var notification = new Notification(title, msg, type);
        (host != null ? host.Notification : UiProperty.Notification).Show(notification, notification.Type,
            classes: ["Light"], onClick: onClick,
            expiration: time ?? TimeSpan.FromSeconds(3.0));
    }

    public static void ShowShortException(string msg, Exception ex)
    {
        Notice($"{msg}\n{ex.Message}", NotificationType.Error);
        // if (Data.SettingEntry.EnableIndependencyWindowNotification)
        // {
        //     NoticeWindow(msg, ex.Message);
        // }
    }

    public static async Task OpenFolder(string path)
    {
        if (Data.DesktopType == DesktopType.MacOs)
        {
            var process = new Process();
            process.StartInfo.FileName = "open";
            process.StartInfo.Arguments = $"\"{path}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
        }
        else
        {
            var launcher = Aurelio.App.TopLevel.Launcher;
            await launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(path));
        }
    }

    public static void ClearDrawer()
    {
        Tasking.Instance.Close();
    }

    public static async Task OpenTaskDrawer(string host)
    {
        var options = new DrawerOptions
        {
            Position = Position.Right,
            Buttons = DialogButton.None,
            CanLightDismiss = true,
            IsCloseButtonVisible = true,
            MinWidth = 470,
            Title = MainLang.TaskingState,
            CanResize = true
        };
         Drawer.Show<TaskCenterDrawer, Tasking>(Tasking.Instance, host, options);
    }

    public static string GetHostId(Control sender)
    {
        var vis = sender.GetVisualRoot();
        return vis is TabWindow w
            ? w.DialogHost.HostId
            : "MainWindow";
    }
}