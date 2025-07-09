using System;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Const;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using Ursa.Controls;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace Aurelio.Public.Module.Ui;

public abstract class Shower
{
    public static async Task<ContentDialogResult> ShowDialogAsync(string title = "Title", string msg = null,
        Control? p_content = null, string b_primary = null, string b_cancel = null, string b_secondary = null,
        TopLevel? p_host = null)
    {
        var content = p_content ?? new SelectableTextBlock()
        {
            TextWrapping = TextWrapping.Wrap,
            FontFamily = (FontFamily)Application.Current.Resources["Font"],
            Text = msg
        };
        if (!string.IsNullOrWhiteSpace(msg) && p_content != null)
        {
            content = new StackPanel()
            {
                Spacing = 15,
                Children =
                {
                    new SelectableTextBlock()
                    {
                        TextWrapping = TextWrapping.Wrap,
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        Text = msg
                    },
                    content
                }
            };
        }

        if (string.IsNullOrWhiteSpace(msg) && p_content == null)
        {
            content = null;
        }

        var dialog = new ContentDialog
        {
            PrimaryButtonText = b_primary,
            Content = content,
            DefaultButton = ContentDialogButton.Primary,
            CloseButtonText = b_cancel,
            SecondaryButtonText = b_secondary,
            FontFamily = (FontFamily)Application.Current.Resources["Font"],
            Title = title
        };
        var result = await dialog.ShowAsync(p_host ?? TopLevel.GetTopLevel(Aurelio.App.UiRoot));
        return result;
    }

    public static void Notice(string msg, NotificationType type = NotificationType.Information,
        Action? onClick = null, bool time = true, string title = "Aurelio")
    {
        var showTitle = "Aurelio";
        if (!string.IsNullOrWhiteSpace(title)) showTitle = title;
        if (time) showTitle += $" - {DateTime.Now:HH:mm:ss}";

        var notification = new Notification(showTitle, msg, type);
        UiProperty.NotificationCards.Insert(0, new NotificationEntry(notification, notification.Type));

        switch (Data.SettingEntry.NoticeWay)
        {
            case Setting.NoticeWay.Bubble:
                NotificationBubble(msg, type, onClick);
                break;
            case Setting.NoticeWay.Card:
                NotificationCard(msg, type, showTitle, onClick);
                break;
        }
    }

    public static void NotificationBubble(string msg, NotificationType type, Action? onClick)
    {
        var toast = new Toast(msg, type);
        UiProperty.Toast.Show(toast, toast.Type /*, classes: ["Light"]*/, onClick: onClick);
    }

    public static void NotificationCard(string msg, NotificationType type, string title, Action? onClick)
    {
        var notification = new Notification(title, msg, type);
        UiProperty.Notification.Show(notification, notification.Type, classes: ["Light"], onClick: onClick);
    }
    
    public static void ShowShortException(string msg, Exception ex)
    {
        Notice($"{msg}\n{ex.Message}", NotificationType.Error);
        // if (Data.SettingEntry.EnableIndependencyWindowNotification)
        // {
        //     NoticeWindow(msg, ex.Message);
        // }
    }
}