using System.IO;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.IO;
using Aurelio.Views.Main;
using Aurelio.Views.Main.Pages.Viewers;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using FluentAvalonia.UI.Controls;

namespace Aurelio.Public.Module.Service;

public class FileNav
{
    public static async Task<bool> NavPage(string path, IAurelioWindow? window = null)
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
                await using var fileStream = File.OpenRead(path);
                return OpenPage(new ImageViewer(name, new Bitmap(fileStream), path), window);
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
            case ".aupkg":
            {
                try
                {
                    var cr = await ShowDialogAsync("安装插件", $"将 {name} 作为 Aurelio 插件安装?",
                        b_primary: MainLang.Ok, b_cancel: MainLang.Cancel);
                    if (cr != ContentDialogResult.Primary) return true;
                    var result = ContentDialogResult.Primary;
                    var t = Path.Combine(ConfigPath.PluginFolderPath, name);
                    if (File.Exists(t))
                    {
                        result = await ShowDialogAsync(
                            title: "插件已存在",
                            msg: $"插件 {name} 已经存在，安装将覆盖现有版本。是否继续？",
                            b_primary: "覆盖安装",
                            b_cancel: "取消"
                        );
                    }

                    if (result == ContentDialogResult.Primary)
                    {
                        File.Copy(path, t, true);
                    }
                    
                    var result1 = await ShowDialogAsync(
                        title: MainLang.NeedRestartApp,
                        msg: $"{name} 安装完成！为了使插件生效，需要重启 Aurelio。是否现在重启？",
                        b_primary: MainLang.RestartNow,
                        b_secondary: MainLang.RestartLater,
                        b_cancel: MainLang.Cancel
                    );

                    switch (result1)
                    {
                        case ContentDialogResult.Primary:
                            // 立即重启
                            Logger.Info($"用户选择立即重启应用以应用 {name} 的安装");
                            AppMethod.RestartApp();
                            break;

                        case ContentDialogResult.Secondary:
                            // 稍后重启，显示通知
                            Notice($"{name} 安装完成，请稍后重启 Aurelio 以应用更改", NotificationType.Success,
                                TimeSpan.FromSeconds(5));
                            Logger.Info($"用户选择稍后重启应用以应用 {name} 的安装");
                            break;

                        default:
                            // 取消，只显示成功通知
                            Notice($"{name} 安装完成", NotificationType.Success);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Notice("安装失败", NotificationType.Error);
                    Logger.Error(e);
                }
                return true;
            }
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