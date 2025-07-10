using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;

namespace Aurelio.Public.Module.IO.Local;

public static class Picker
{
    public static async Task<IReadOnlyList<string>> PickFolderAsync(this IStorageProvider StorageProvider,
        FolderPickerOpenOptions options)
    {
        if (Data.SettingEntry.UseFilePicker)
            return (await StorageProvider.OpenFolderPickerAsync(options)).Select(x => x.Path.LocalPath).ToList();
        var text = new TextBox { Watermark = MainLang.InputFolderPath, Text = string.Empty};
        return await ShowDialogAsync(options.Title ?? "Aurelio", null, text, MainLang.Ok, MainLang.Cancel) ==
               ContentDialogResult.Primary && !text.Text.IsNullOrWhiteSpace() ? [text.Text]! : [];
    }

    public static async Task<IReadOnlyList<string>> PickFileAsync(this IStorageProvider StorageProvider,
        FilePickerOpenOptions options)
    {
        if (Data.SettingEntry.UseFilePicker)
            return (await StorageProvider.OpenFilePickerAsync(options)).Select(x => x.Path.LocalPath).ToList();
        var text = new TextBox { Watermark = MainLang.InputFilePath, Text = string.Empty};
        return await ShowDialogAsync(options.Title ?? "Aurelio", null, text, MainLang.Ok, MainLang.Cancel) ==
            ContentDialogResult.Primary && !text.Text.IsNullOrWhiteSpace() ? [text.Text]! : [];
    }

    public static async Task<string> PickSaveFileAsync(this IStorageProvider StorageProvider,
        FilePickerSaveOptions options)
    {
        if (Data.SettingEntry.UseFilePicker)
            return (await StorageProvider.SaveFilePickerAsync(options)).Path.LocalPath;
        var text = new TextBox { Watermark = MainLang.InputFilePath, Text = string.Empty};
        return await ShowDialogAsync(options.Title ?? "Aurelio", null, text, MainLang.Ok, MainLang.Cancel) ==
            ContentDialogResult.Primary && !text.Text.IsNullOrWhiteSpace() ? text.Text : null;
    }
}