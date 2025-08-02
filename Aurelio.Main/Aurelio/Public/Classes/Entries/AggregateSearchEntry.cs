using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Avalonia.Media;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.IO;

namespace Aurelio.Public.Classes.Entries;

public class AggregateSearchEntry : ReactiveObject
{
    [Reactive] public object Type { get; set; }
    [Reactive] public StreamGeometry Icon { get; set; }
    [Reactive] public int Order { get; set; }
    [Reactive] public string Title { get; set; }
    [Reactive] public string Tag { get; set; }
    [Reactive] public string Summary { get; set; }
    [Reactive] public object OriginObject { get; set; }

    public AggregateSearchEntry()
    {
    }

    public AggregateSearchEntry(IAurelioTabPage entry, string? summary = null, string? tag = null)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Title = $"{entry.PageInfo.Title}";
            Summary = summary ?? $"{MainLang.OpenOrTogglePage}";
            OriginObject = entry;
            Type = AggregateSearchEntryType.AurelioTabPage;
            Tag = tag;
            Icon = StreamGeometry.Parse(
                "F1 M 12.670898 5.825195 L 15 6.796875 L 15.97168 9.125977 C 16.025391 9.228516 16.132812 9.296875 16.25 9.296875 C 16.367188 9.296875 16.474609 9.228516 16.52832 9.125977 L 17.5 6.796875 L 19.829102 5.825195 C 19.931641 5.771484 20 5.664062 20 5.546875 C 20 5.429688 19.931641 5.322266 19.829102 5.268555 L 17.5 4.296875 L 16.52832 1.967773 C 16.474609 1.865234 16.367188 1.796875 16.25 1.796875 C 16.132812 1.796875 16.025391 1.865234 15.97168 1.967773 L 15 4.296875 L 12.670898 5.268555 C 12.568359 5.322266 12.5 5.429688 12.5 5.546875 C 12.5 5.664062 12.568359 5.771484 12.670898 5.825195 Z M 19.829102 17.768555 L 17.5 16.796875 L 16.52832 14.467773 C 16.474609 14.365234 16.367188 14.296875 16.25 14.296875 C 16.132812 14.296875 16.025391 14.365234 15.97168 14.467773 L 15 16.796875 L 12.670898 17.768555 C 12.568359 17.822266 12.5 17.929688 12.5 18.046875 C 12.5 18.164062 12.568359 18.271484 12.670898 18.325195 L 15 19.296875 L 15.97168 21.625977 C 16.025391 21.728516 16.132812 21.796875 16.25 21.796875 C 16.367188 21.796875 16.474609 21.728516 16.52832 21.625977 L 17.5 19.296875 L 19.829102 18.325195 C 19.931641 18.271484 20 18.164062 20 18.046875 C 20 17.929688 19.931641 17.822266 19.829102 17.768555 Z M 15 11.782227 C 15 11.547852 14.868164 11.328125 14.65332 11.220703 L 10.258789 9.018555 L 8.056641 4.614258 C 7.84668 4.189453 7.15332 4.189453 6.943359 4.614258 L 4.741211 9.018555 L 0.34668 11.220703 C 0.131836 11.328125 0 11.547852 0 11.782227 C 0 12.021484 0.131836 12.236328 0.34668 12.34375 L 4.741211 14.545898 L 6.943359 18.950195 C 7.045898 19.160156 7.265587 19.296875 7.5 19.296875 C 7.734337 19.296875 7.954102 19.160156 8.056641 18.950195 L 10.258789 14.545898 L 14.65332 12.34375 C 14.868164 12.236328 15 12.021484 15 11.782227 Z ");
            Order = 300;
        });
    }

    public AggregateSearchEntry(FileSystemInfo fileSystemInfo, bool isRoot = false)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            OriginObject = fileSystemInfo;
            Type = AggregateSearchEntryType.SystemFile;
            Order = 400;

            if (fileSystemInfo is DirectoryInfo dirInfo)
            {
                Title = isRoot ? dirInfo.FullName : dirInfo.Name;
                Summary = isRoot ? MainLang.SystemDrive : $"{MainLang.Folder} • {dirInfo.FullName}";
                Icon = StreamGeometry.Parse(
                    "M64 480H448c35.3 0 64-28.7 64-64V160c0-35.3-28.7-64-64-64H288c-10.1 0-19.6-4.7-25.6-12.8L243.2 57.6C231.1 41.5 212.1 32 192 32H64C28.7 32 0 60.7 0 96V416c0 35.3 28.7 64 64 64z");
            }
            else if (fileSystemInfo is FileInfo fileInfo)
            {
                Title = fileInfo.Name;
                var sizeText = fileInfo.Length < 1024 ? $"{fileInfo.Length} B" :
                    fileInfo.Length < 1024 * 1024 ? $"{fileInfo.Length / 1024} KB" :
                    $"{fileInfo.Length / (1024 * 1024)} MB";
                Summary = $"{MainLang.File} • {sizeText} • {fileInfo.FullName}";
                Icon = StreamGeometry.Parse(
                    "M64 0C28.7 0 0 28.7 0 64L0 448c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-288-128 0c-17.7 0-32-14.3-32-32L224 0 64 0zM256 0l0 128 128 0L256 0zM112 256l160 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-160 0c-8.8 0-16-7.2-16-16s7.2-16 16-16zm0 64l160 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-160 0c-8.8 0-16-7.2-16-16s7.2-16 16-16zm0 64l160 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-160 0c-8.8 0-16-7.2-16-16s7.2-16 16-16z");
            }
        });
    }

    public AggregateSearchEntry(string parentPath)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            OriginObject = parentPath;
            Type = AggregateSearchEntryType.SystemFileGoUp;
            Order = 500; // Higher order to appear first
            Title = $".. ({MainLang.BackToUpLevel})";

            if (string.IsNullOrEmpty(parentPath))
            {
                Summary = MainLang.SystemDrive;
            }
            else
            {
                Summary = $"{MainLang.Folder} • {parentPath}";
            }

            Icon = StreamGeometry.Parse(
                "F1 M 16.25 18.046875 L 3.75 18.046875 C 3.059654 18.046875 2.5 18.606529 2.5 19.296875 L 2.5 19.296875 C 2.5 19.987221 3.059654 20.546875 3.75 20.546875 L 16.25 20.546875 C 16.940346 20.546875 17.5 19.987221 17.5 19.296875 L 17.5 19.296875 C 17.5 18.606529 16.940346 18.046875 16.25 18.046875 Z M 4.6875 10.539742 L 7.5 10.539742 L 7.5 15.543289 C 7.5 16.234169 8.059692 16.794167 8.75 16.794167 L 11.25 16.794167 C 11.940384 16.794167 12.5 16.234169 12.5 15.543289 L 12.5 10.539742 L 15.3125 10.539742 C 15.686646 10.539742 16.025391 10.316811 16.173706 9.972878 C 16.321411 9.629021 16.251221 9.229584 15.994263 8.957138 L 10.681763 3.328133 C 10.327759 2.95311 9.672241 2.95311 9.318237 3.328133 L 4.005737 8.957138 C 3.748779 9.229584 3.678589 9.629021 3.826294 9.972878 C 3.974609 10.316811 4.313354 10.539742 4.6875 10.539742 Z "); // Up arrow icon
        });
    }
}