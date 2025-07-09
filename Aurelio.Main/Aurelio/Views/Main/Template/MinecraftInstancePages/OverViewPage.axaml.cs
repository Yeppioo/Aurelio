using System.IO;
using Aurelio.Public.Classes.Enum.Minecraft;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;

namespace Aurelio.Views.Main.Template.MinecraftInstancePages;

public partial class OverViewPage : PageMixModelBase, IAurelioPage
{
    public RecordMinecraftEntry Entry { get; }

    public OverViewPage(RecordMinecraftEntry entry)
    {
        InitializeComponent();
        Entry = entry;
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
    }

    private void BindingEvent()
    {
        EditMinecraftId.Click += async (_, _) =>
        {
            var text = new TextBox { Text = Entry.Id };
            var d = await ShowDialogAsync(MainLang.Rename, p_content: text, b_primary: MainLang.Ok,
                b_cancel: MainLang.Cancel);
            if (d != ContentDialogResult.Primary) return;
        };
    }

    public Border RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
}