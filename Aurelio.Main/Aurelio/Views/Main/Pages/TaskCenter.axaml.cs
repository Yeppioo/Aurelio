using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;

namespace Aurelio.Views.Main.Pages;

public partial class TaskCenter : PageMixModelBase, IAurelioTabPage
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
        Tasking.Tasks.CollectionChanged += (_, _) =>
        {
            PageInfo.Title = MainLang.TaskingTip.Replace("{num}", Tasking.Tasks.Count.ToString());
        };
        DataContext = Tasking.Instance;
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
}