using System.IO;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Views.Main;
using Aurelio.Views.Main.Pages;
using Avalonia.Media.Imaging;
using ImageViewer = Aurelio.Views.Main.Pages.Viewers.ImageViewer;
using LogViewer = Aurelio.Views.Main.Pages.Viewers.LogViewer;

namespace Aurelio.Public.Module.Service;

public class FileNav
{
    public static bool NavPage(string path, IAurelioWindow? window = null)
    {
        var extension = Path.GetExtension(path).ToLower();
        var name = Path.GetFileName(path);
        switch (extension)
        {
            case ".png":
            case ".jpg":
            case ".jpeg":
            case ".webp":
            {
                using var fileStream = File.OpenRead(path);
                OpenPage(new ImageViewer(name, Bitmap.DecodeToWidth(fileStream, 1080), path), window);
                return true;
            }
            case ".log":
            {
                OpenPage(new LogViewer(path, name), window);
                return true;
            }
        }

        return false;
    }

    public static void OpenPage(IAurelioTabPage page, IAurelioWindow? window = null)
    {
        if ((window ?? UiProperty.ActiveWindow)is TabWindow tabWindow)
        {
            tabWindow.CreateTab(new TabEntry(page));
            return;
        }

        Aurelio.App.UiRoot.CreateTab(new TabEntry(page));
    }
}