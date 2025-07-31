using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;

namespace Aurelio.Public.Classes.Entries;

public class NugetSearchResult : ReactiveObject
{
    /// <summary>
    /// 包ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 包标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 包描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 作者列表
    /// </summary>
    public string[] Authors { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 作者字符串（用于显示）
    /// </summary>
    public string AuthorsString => Authors.Length > 0 ? string.Join(", ", Authors) : "Unknown";

    /// <summary>
    /// 总下载量
    /// </summary>
    public long TotalDownloads { get; set; }

    /// <summary>
    /// 格式化的下载量字符串
    /// </summary>
    public string TotalDownloadsString
    {
        get
        {
            if (TotalDownloads >= 1_000_000)
                return $"{TotalDownloads / 1_000_000.0:F1}M";
            if (TotalDownloads >= 1_000)
                return $"{TotalDownloads / 1_000.0:F1}K";
            return TotalDownloads.ToString();
        }
    }

    private string _latestVersion = string.Empty;

    /// <summary>
    /// 最新版本
    /// </summary>
    public string LatestVersion
    {
        get => _latestVersion;
        set
        {
            _latestVersion = value;
            // 如果SelectedVersion为空，自动设置为最新版本
            if (string.IsNullOrEmpty(SelectedVersion))
            {
                SelectedVersion = value;
            }
        }
    }

    /// <summary>
    /// 所有可用版本
    /// </summary>
    [Reactive] public ObservableCollection<string> AllVersions { get; set; } = new();

    /// <summary>
    /// 当前选择的版本
    /// </summary>
    [Reactive] public string SelectedVersion { get; set; } = string.Empty;

    /// <summary>
    /// 是否正在加载版本信息
    /// </summary>
    [Reactive] public bool IsLoadingVersions { get; set; } = false;

    /// <summary>
    /// 是否已展开显示详细信息
    /// </summary>
    [Reactive] public bool IsExpanded { get; set; } = false;

    /// <summary>
    /// 是否正在安装
    /// </summary>
    [Reactive] public bool IsInstalling { get; set; } = false;

    /// <summary>
    /// 是否正在下载
    /// </summary>
    [Reactive] public bool IsDownloading { get; set; } = false;

    /// <summary>
    /// 包图标URL
    /// </summary>
    public string IconUrl { get; set; } = string.Empty;

    /// <summary>
    /// 项目URL
    /// </summary>
    public string ProjectUrl { get; set; } = string.Empty;

    /// <summary>
    /// 许可证URL
    /// </summary>
    public string LicenseUrl { get; set; } = string.Empty;

    /// <summary>
    /// 标签
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 标签字符串（用于显示）
    /// </summary>
    public string TagsString => Tags.Length > 0 ? string.Join(", ", Tags) : string.Empty;

    /// <summary>
    /// 是否为预发布版本
    /// </summary>
    public bool IsPrerelease { get; set; } = false;

    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTime? Published { get; set; }

    /// <summary>
    /// 格式化的发布时间
    /// </summary>
    public string PublishedString => Published?.ToString("yyyy-MM-dd") ?? "Unknown";

    public NugetSearchResult()
    {
        // 当选择版本改变时，更新相关状态
        this.WhenAnyValue(x => x.SelectedVersion)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(SelectedVersion)));
    }
}
