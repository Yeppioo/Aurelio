using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aurelio.Public.Langs;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;

namespace Aurelio.Public.Module.IO.Local;

public static class Picker
{
    public static async Task<IReadOnlyList<string>> PickFolderAsync(this Control sender,
        FolderPickerOpenOptions options)
    {
        var StorageProvider = TopLevel.GetTopLevel(sender).StorageProvider;
        if (Data.SettingEntry.UseFilePicker)
            return (await StorageProvider.OpenFolderPickerAsync(options)).Select(x => x.Path.LocalPath).ToList();
        var text = new TextBox { Watermark = MainLang.InputFolderPath, Text = string.Empty };
        return await ShowDialogAsync(options.Title ?? "Aurelio", null, text, MainLang.Ok, MainLang.Cancel,
                sender: sender) ==
            ContentDialogResult.Primary && !text.Text.IsNullOrWhiteSpace()
                ? [text.Text]!
                : [];
    }

    public static async Task<IReadOnlyList<string>> PickFileAsync(this Control sender,
        FilePickerOpenOptions options)
    {
        var StorageProvider = TopLevel.GetTopLevel(sender).StorageProvider;
        if (Data.SettingEntry.UseFilePicker)
            return (await StorageProvider.OpenFilePickerAsync(options)).Select(x => x.Path.LocalPath).ToList();
        var text = new TextBox { Watermark = MainLang.InputFilePath, Text = string.Empty };
        return await ShowDialogAsync(options.Title ?? "Aurelio", null, text, MainLang.Ok, MainLang.Cancel,
                sender: sender) ==
            ContentDialogResult.Primary && !text.Text.IsNullOrWhiteSpace()
                ? [text.Text]!
                : [];
    }

    public static async Task<string> PickSaveFileAsync(this Control sender,
        FilePickerSaveOptions options)
    {
        var StorageProvider = TopLevel.GetTopLevel(sender).StorageProvider;
        if (Data.SettingEntry.UseFilePicker)
            return (await StorageProvider.SaveFilePickerAsync(options)).Path.LocalPath;
        var text = new TextBox { Watermark = MainLang.InputFilePath, Text = string.Empty };
        return await ShowDialogAsync(options.Title ?? "Aurelio", null, text, MainLang.Ok, MainLang.Cancel,
                sender: sender) ==
            ContentDialogResult.Primary && !text.Text.IsNullOrWhiteSpace()
                ? text.Text
                : null;
    }
}