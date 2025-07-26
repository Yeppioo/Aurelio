using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Setter = Aurelio.Public.Module.IO.Local.Setter;

namespace Aurelio.Plugin.Minecraft.Classes.Minecraft;

public class MinecraftLocalSaveEntry : ReactiveObject
{
    [Reactive] public string Name { get; set; }
    [Reactive] public string Path { get; set; }
    [Reactive] public Bitmap Icon { get; set; }
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public SaveInfo SaveInfo { get; set; }
    [Reactive] public Action Callback { get; set; }

    public async Task Delete(Control sender)
    {
        var text = $"• {System.IO.Path.GetFileName(Name)}";

        var title = Data.DesktopType == DesktopType.Windows
            ? MainLang.MoveToRecycleBin
            : MainLang.DeleteSelect;
        var dialog = await Overlay.ShowDialogAsync(title, text, b_cancel: MainLang.Cancel,
            b_primary: MainLang.Ok, sender: sender);
        if (dialog != ContentDialogResult.Primary) return;

        if (Data.DesktopType == DesktopType.Windows)
            FileSystem.DeleteDirectory(Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
        else
            Directory.Delete(Path);

        Callback?.Invoke();
    }

    public async Task ShowInfo(Control sender)
    {
        var text =
            $"{MainLang.Name}: {SaveInfo.FolderName}\n" +
            $"{MainLang.Seed}: {SaveInfo.Seed}\n" +
            $"{MainLang.GameVersion}: {SaveInfo.Version}\n" +
            $"{MainLang.AllowCommands}: {SaveInfo.AllowCommands}\n" +
            $"{MainLang.GameType}: {SaveInfo.GameType}\n" +
            $"{MainLang.CreateTime}: {SaveInfo.CreationTime}\n" +
            $"{MainLang.LastPlayTime}: {SaveInfo.LastPlayTime}\n" +
            $"{MainLang.LastModifiedTime}: {SaveInfo.LastWriteTime}\n" +
            $"{MainLang.PlayerCount}: {SaveInfo.DatFileCount}\n" +
            $"{MainLang.DataPackCount}: {SaveInfo.ZipFileCount}";
        await Overlay.ShowDialogAsync(MainLang.SaveInfo, text, b_primary: MainLang.Ok, sender: sender);
        Callback?.Invoke();
    }

    public void OpenFolder()
    {
        var path = SaveInfo.FolderPath;
        Setter.TryCreateFolder(path);
        _ = Overlay.OpenFolder(path);
    }
}

public class SaveInfo
{
    public string FolderName { get; init; }
    public string FolderPath { get; init; }
    public DateTime CreationTime { get; init; }
    public DateTime LastWriteTime { get; init; }
    public DateTime LastPlayTime { get; init; }
    public Bitmap IconBitmap { get; set; }
    public int DatFileCount { get; init; }
    public int ZipFileCount { get; init; }
    public string Version { get; init; }
    public long Seed { get; init; }
    public int GameType { get; init; }
    public bool AllowCommands { get; init; }
}