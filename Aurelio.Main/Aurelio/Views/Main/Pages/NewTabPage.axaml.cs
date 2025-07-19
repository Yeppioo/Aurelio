using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.Service;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;

namespace Aurelio.Views.Main.Pages;

public partial class NewTabPage : PageMixModelBase, IAurelioTabPage
{
    public DateTime _currentTime = DateTime.Now;
    public string _currentLunarMonthDay = string.Empty;
    public string _currentLunarYear = string.Empty;
    public string _currentWeekDay = string.Empty;

    public DateTime CurrentTime
    {
        get => _currentTime;
        set
        {
            SetField(ref _currentTime, value);
            UpdateLunarDate(CurrentTime);
            UpdateWeekDay(CurrentTime);
        }
    }

    public string CurrentLunarMonthDay
    {
        get => _currentLunarMonthDay;
        set => SetField(ref _currentLunarMonthDay, value);
    }

    public string CurrentLunarYear
    {
        get => _currentLunarYear;
        set => SetField(ref _currentLunarYear, value);
    }

    public string CurrentWeekDay
    {
        get => _currentWeekDay;
        set => SetField(ref _currentWeekDay, value);
    }
    
    public NewTabPage()
    {
        InitializeComponent();
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        PageInfo = new PageInfoEntry
        {
            Title = MainLang.NewTab,
            Icon = StreamGeometry.Parse(
                "F1 M 12.670898 5.825195 L 15 6.796875 L 15.97168 9.125977 C 16.025391 9.228516 16.132812 9.296875 16.25 9.296875 C 16.367188 9.296875 16.474609 9.228516 16.52832 9.125977 L 17.5 6.796875 L 19.829102 5.825195 C 19.931641 5.771484 20 5.664062 20 5.546875 C 20 5.429688 19.931641 5.322266 19.829102 5.268555 L 17.5 4.296875 L 16.52832 1.967773 C 16.474609 1.865234 16.367188 1.796875 16.25 1.796875 C 16.132812 1.796875 16.025391 1.865234 15.97168 1.967773 L 15 4.296875 L 12.670898 5.268555 C 12.568359 5.322266 12.5 5.429688 12.5 5.546875 C 12.5 5.664062 12.568359 5.771484 12.670898 5.825195 Z M 19.829102 17.768555 L 17.5 16.796875 L 16.52832 14.467773 C 16.474609 14.365234 16.367188 14.296875 16.25 14.296875 C 16.132812 14.296875 16.025391 14.365234 15.97168 14.467773 L 15 16.796875 L 12.670898 17.768555 C 12.568359 17.822266 12.5 17.929688 12.5 18.046875 C 12.5 18.164062 12.568359 18.271484 12.670898 18.325195 L 15 19.296875 L 15.97168 21.625977 C 16.025391 21.728516 16.132812 21.796875 16.25 21.796875 C 16.367188 21.796875 16.474609 21.728516 16.52832 21.625977 L 17.5 19.296875 L 19.829102 18.325195 C 19.931641 18.271484 20 18.164062 20 18.046875 C 20 17.929688 19.931641 17.822266 19.829102 17.768555 Z M 15 11.782227 C 15 11.547852 14.868164 11.328125 14.65332 11.220703 L 10.258789 9.018555 L 8.056641 4.614258 C 7.84668 4.189453 7.15332 4.189453 6.943359 4.614258 L 4.741211 9.018555 L 0.34668 11.220703 C 0.131836 11.328125 0 11.547852 0 11.782227 C 0 12.021484 0.131836 12.236328 0.34668 12.34375 L 4.741211 14.545898 L 6.943359 18.950195 C 7.045898 19.160156 7.265587 19.296875 7.5 19.296875 C 7.734337 19.296875 7.954102 19.160156 8.056641 18.950195 L 10.258789 14.545898 L 14.65332 12.34375 C 14.868164 12.236328 15 12.021484 15 11.782227 Z ")
        };
        DataContext = this;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5) };
        timer.Tick += (_, _) => CurrentTime = DateTime.Now;
        timer.Start();
        Loaded += (_, _) =>
        {
            Filter();
        };
        SearchBox.GotFocus += (_, _) =>
        {
            Filter();
        };
        AggregateSearchListBox.SelectionChanged += (s, _) =>
        {
            if (AggregateSearchListBox.SelectedItem is not AggregateSearchEntry entry) return;
            AggregateSearch.Execute(entry, Root);
            if (entry.Type == AggregateSearchEntryType.MinecraftAccount) return;
            var r = TopLevel.GetTopLevel(Root);
            if (r is TabWindow window)
            {
                window.RemoveTab(HostTab);
            }
            else
            {
                App.UiRoot.ViewModel.Tabs.Remove(HostTab);
            }
        };
    }

    private string _aggregateSearchFilter = "";
    public ObservableCollection<AggregateSearchEntry> FilteredAggregateSearchEntries { get; } = [];

    public string AggregateSearchFilter
    {
        get => _aggregateSearchFilter;
        set
        {
            SetField(ref _aggregateSearchFilter, value);
            Filter();
        }
    }

    private void Filter()
    {
        try
        {
            FilteredAggregateSearchEntries.Clear();
            FilteredAggregateSearchEntries.AddRange(Data.AggregateSearchEntries.Where(item =>
                    item.Title.Contains(AggregateSearchFilter, StringComparison.OrdinalIgnoreCase) ||
                    item.Summary.Contains(AggregateSearchFilter, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Order).ThenBy(x => x.Title));
        }
        catch
        {
            // ignored
        }
    }

    private void UpdateLunarDate(DateTime date)
    {
        var chineseEra = new ChineseLunisolarCalendar();
        int month = chineseEra.GetMonth(date);
        int day = chineseEra.GetDayOfMonth(date);
        int year = chineseEra.GetYear(date);

        CurrentLunarMonthDay = ToChineseMonth(month) + ToChineseDay(day);
        CurrentLunarYear = ToChineseYear(year);
    }

    private void UpdateWeekDay(DateTime date)
    {
        string[] weekDays = ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"];
        CurrentWeekDay = weekDays[(int)date.DayOfWeek];
    }


    private static string ToChineseMonth(int month)
    {
        string[] months = ["正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "冬", "腊"];
        return month is >= 1 and <= 12 ? months[month - 1] + "月" : "";
    }

    private static string ToChineseDay(int day)
    {
        if (day is >= 1 and <= 10)
            return "初" + "一二三四五六七八九十"[day - 1];
        if (day == 20)
            return "二十";
        if (day is >= 21 and <= 29)
            return "廿" + "一二三四五六七八九"[day - 21];
        return day == 30 ? "三十" : "";
    }

    private static string ToChineseYear(int lunarYear)
    {
        // 天干：甲乙丙丁戊己庚辛壬癸
        string[] heavenlyStems = ["甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸"];
        // 地支：子丑寅卯辰巳午未申酉戌亥
        string[] earthlyBranches = ["子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥"];

        // 农历年份从1开始，对应甲子年
        var stemIndex = (lunarYear - 1) % 10;
        var branchIndex = (lunarYear - 1) % 12;

        return heavenlyStems[stemIndex] + earthlyBranches[branchIndex] + "年";
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public void OnClose()
    {
    }
}