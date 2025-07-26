using Aurelio.Plugin.Minecraft.Classes.Minecraft;
using Aurelio.Plugin.Minecraft.Service.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.IO.Local;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;

namespace Aurelio.Plugin.Minecraft.Operate;

public class MinecraftFolder
{
    public static async Task AddByUi(Control sender)
    {
        var list = await sender.PickFolderAsync(
            new FolderPickerOpenOptions { Title = MainLang.SelectMinecraftFolder, AllowMultiple = true });
        if (list.Count < 1) return;
        foreach (var p in list)
        {
            var path = p;
            path = path.Trim().TrimEnd(Path.DirectorySeparatorChar);
            var folder = Path.GetFileName(path);
            var parentDirectoryPath = Path.GetDirectoryName(path);
            var name = string.Empty;
            if (parentDirectoryPath != null) name = Path.GetFileName(parentDirectoryPath);

            if (folder != ".minecraft")
                if (Directory.Exists(Path.Combine(path, ".minecraft")))
                {
                    path = Path.Combine(path, ".minecraft");
                    name = folder;
                }

            var textbox = new TextBox
            {
                Watermark = MainLang.DisplayName,
                TextWrapping = TextWrapping.Wrap,
                MaxLength = 60, Text = name
            };
            var textbox1 = new TextBox
            {
                Text = path,
                TextWrapping = TextWrapping.Wrap
            };
            var dialog = new ContentDialog
            {
                Title = MainLang.AddFolder,
                Content = new StackPanel
                {
                    Spacing = 10,
                    Children =
                    {
                        textbox,
                        textbox1
                    }
                },
                PrimaryButtonText = MainLang.Ok,
                CloseButtonText = MainLang.Cancel,
                IsPrimaryButtonEnabled = false
            };
            dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(textbox.Text);
            textbox.TextChanged += (_, _) =>
            {
                dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(textbox.Text);
            };
            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;
            var entry = new RecordMinecraftFolderEntry
                { Name = textbox.Text, Path = textbox1.Text };
            MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftFolderEntries.Add(entry);
        }
        
        AppMethod.SaveSetting();
        _ = MinecraftInstancesHandler.Load(MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftFolderEntries
            .Select(x => x.Path).ToArray());
    }
}