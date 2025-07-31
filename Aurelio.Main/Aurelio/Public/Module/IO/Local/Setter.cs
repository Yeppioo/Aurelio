using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aurelio.Public.Langs;
using Avalonia.Layout;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;

namespace Aurelio.Public.Module.IO.Local;

public abstract class Setter
{
    public static void TryCreateFolder(string path)
    {
        if (Directory.Exists(path)) return;
        var directoryInfo = new DirectoryInfo(path);
        directoryInfo.Create();
    }

    public static void ClearFolder(string folderPath, string[]? ignore = null)
    {
        if (ignore != null && ignore.Contains(folderPath)) return;
        if (!Directory.Exists(folderPath)) return;

        foreach (var file in Directory.GetFiles(folderPath)) File.Delete(file);

        foreach (var dir in Directory.GetDirectories(folderPath))
        {
            ClearFolder(dir, ignore);
            Directory.Delete(dir);
        }
    }

    public static void TryClearFolder(string folderPath, string[]? ignore = null)
    {
        try
        {
            ClearFolder(folderPath, ignore);
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    public static async Task<bool> CopyFileWithDialog(string source, string target)
    {
        var path = target;
        if (File.Exists(target))
        {
            var cr = await ShowDialogAsync($"{MainLang.Conflict}: {Path.GetFileName(source)}", MainLang.FileConflictTip,
                b_primary: MainLang.Cover,
                b_secondary: MainLang.Rename, b_cancel: MainLang.Cancel);
            if (cr == ContentDialogResult.Primary)
            {
                if (source == path) return false;
                File.Copy(source, target, true);
            }
            else if (cr == ContentDialogResult.None)
            {
                return false;
            }
            else
            {
                var textBox = new TextBox
                {
                    FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap,
                    Text = Path.GetFileName(target), HorizontalAlignment = HorizontalAlignment.Stretch, Width = 500
                };
                var cr1 = await ShowDialogAsync(MainLang.Rename, p_content: textBox, b_cancel: MainLang.Cancel,
                    b_primary: MainLang.Ok);
                if (cr1 != ContentDialogResult.Primary) return false;
                path = Path.Combine(Path.GetDirectoryName(target)!, textBox.Text);
                return await CopyFileWithDialog(source, path);
            }
        }
        else
        {
            if (source == path) return false;
            File.Copy(source, path, true);
        }

        return true;
    }
}