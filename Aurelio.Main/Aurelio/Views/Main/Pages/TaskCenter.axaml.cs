using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia.VisualTree;

namespace Aurelio.Views.Main.Pages;

public partial class TaskCenter : PageMixModelBase, IAurelioTabPage, IAurelioNavPage
{
    public TaskCenter()
    {
        InitializeComponent();
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        PageInfo = new PageInfoEntry
        {
            Title = MainLang.TaskingTip.Replace("{num}", Tasking.Tasks.Count.ToString()),
            Icon = Icons.Model3D
        };
        ShortInfo = $"{MainLang.TaskCenter} / {MainLang.TaskingTip.Replace("{num}", Tasking.Tasks.Count.ToString())}";
        Tasking.Tasks.CollectionChanged += (_, _) =>
        {
            PageInfo.Title = MainLang.TaskingTip.Replace("{num}", Tasking.Tasks.Count.ToString());
            ShortInfo = $"{MainLang.TaskCenter} / {MainLang.TaskingTip.Replace("{num}", Tasking.Tasks.Count.ToString())}";
        };
        DataContext = Tasking.Instance;
    }

    private string _shortInfo = string.Empty;

    public string ShortInfo
    {
        get => _shortInfo;
        set => SetField(ref _shortInfo, value);
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public void OnClose()
    {
    }

    private void AvaloniaObject_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
    }

    public static AurelioStaticPageInfo StaticPageInfo { get; } = new()
    {
        Title = MainLang.TaskCenter,
        Icon = Icons.Model3D,
        NeedPath = false,
        AutoCreate = true
    };

    public static IAurelioNavPage Create((object sender, object? param) t)
    {
        var root = ((Control)t.sender).GetVisualRoot();
        if (root is TabWindow tabWindow)
        {
            tabWindow.CreateTab(new TabEntry(new TaskCenter()));
            return null;
        }

        App.UiRoot.CreateTab(new TabEntry(new TaskCenter()));
        return null;
    }
}