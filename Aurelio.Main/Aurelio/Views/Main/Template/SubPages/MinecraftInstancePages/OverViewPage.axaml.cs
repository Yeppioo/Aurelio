using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
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

    public RecordMinecraftEntry Entry { get; }
    public ObservableCollection<RecordJavaRuntime> JavaRuntimes { get; } = [];

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }

    private void BindingEvent()
    {
        EditMinecraftId.Click += async (_, _) =>
        {
            var text = new TextBox { Text = Entry.Id };
            var d = await ShowDialogAsync(MainLang.Rename, p_content: text, b_primary: MainLang.Ok,
                b_cancel: MainLang.Cancel);
            if (d != ContentDialogResult.Primary) return;
        };
        Loaded += (_, _) =>
        {
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
    }
}