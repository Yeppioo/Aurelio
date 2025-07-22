using System.IO;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Views.Main;
using Aurelio.Views.Main.Pages;
using Aurelio.Views.Main.Pages.Viewers;
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
                return OpenPage(new ImageViewer(name, Bitmap.DecodeToWidth(fileStream, 1080), path), window);
            }
            case ".log":
                return OpenPage(new LogViewer(path, name), window);
            case ".json":
                return OpenPage(new JsonViewer(path, name), window);
            case ".zip":
            case ".7z":
            case ".rar":
            case ".tar":
            case ".gz":
                return OpenPage(new ZipViewer(name, path), window);
            case ".cs":
            case ".cpp":
            case ".cc":
            case ".cxx":
            case ".c++":
            case ".c":
            case ".h":
            case ".hpp":
            case ".js":
            case ".mjs":
            case ".ts":
            case ".svg":
            case ".tsx":
            case ".py":
            case ".pyw":
            case ".java":
            case ".go":
            case ".rs":
            case ".rb":
            case ".php":
            case ".swift":
            case ".dart":
            case ".fs":
            case ".fsx":
            case ".vb":
            case ".r":
            case ".lua":
            case ".pl":
            case ".pm":
            case ".pas":
            case ".clj":
            case ".cljs":
            case ".coffee":
            case ".groovy":
            case ".jl":
            case ".html":
            case ".htm":
            case ".css":
            case ".scss":
            case ".less":
            case ".pug":
            case ".hbs":
            case ".handlebars":
            case ".razor":
            case ".xml":
            case ".yaml":
            case ".yml":
            case ".md":
            case ".markdown":
            case ".tex":
            case ".txt":
            case ".typ":
            case ".ini":
            case ".adoc":
            case ".asciidoc":
            case ".sh":
            case ".bash":
            case ".zsh":
            case ".bat":
            case ".cmd":
            case ".ps1":
            case ".sql":
            case ".diff":
            case ".patch":
            case ".dockerfile":
            case ".gitignore":
            case ".gitattributes":
            case ".hlsl":
            case ".shader":
            case ".make":
            case ".axaml":
            case ".mk":
                return OpenPage(new CodeViewer(name, path), window);
        }

        return false;
    }

    public static bool OpenPage(IAurelioTabPage page, IAurelioWindow? window = null)
    {
        if ((window ?? UiProperty.ActiveWindow) is TabWindow tabWindow)
        {
            tabWindow.CreateTab(new TabEntry(page));
            return true;
        }

        Aurelio.App.UiRoot.CreateTab(new TabEntry(page));
        return true;
    }
}