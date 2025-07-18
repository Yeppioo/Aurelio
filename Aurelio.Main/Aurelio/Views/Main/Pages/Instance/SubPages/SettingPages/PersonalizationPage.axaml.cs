using System.Collections.ObjectModel;
using System.IO;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia.Platform.Storage;

namespace Aurelio.Views.Main.Pages.Instance.SubPages.SettingPages;

public partial class PersonalizationPage : PageMixModelBase, IAurelioPage
{
    public ObservableCollection<Language> Langs { get; } =
    [
        new() { Label = "简体中文", Code = "zh-CN" },
        new() { Label = "繁體中文", Code = "zh-Hant" },
        new() { Label = "English", Code = "en" },
    ];
    public PersonalizationPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
    }
    public PageLoadingAnimator InAnimator { get; set; }
    public Control RootElement { get; set; }

    public async Task SetBackGroundImg()
    {
        var list = await this.PickFileAsync(
            new FilePickerOpenOptions { AllowMultiple = false, FileTypeFilter = [FilePickerFileTypes.ImageAll] });
        if (list.Count == 0) return;
        Data.SettingEntry.BackGroundImgData =
            Public.Module.Value.Converter.BytesToBase64(await File.ReadAllBytesAsync(list[0]));
    }
}