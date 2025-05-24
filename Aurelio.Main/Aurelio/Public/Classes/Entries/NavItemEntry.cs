using Aurelio.ViewModels;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Aurelio.Public.Classes.Entries;

public partial class NavItemEntry : ViewModelBase
{
    public NavItemEntry(string title, object content, StreamGeometry icon = null, bool canClose = false)
    {
        CanClose = canClose;
        Title = title;
        Icon = icon;
        Content = content;
    }

    [ObservableProperty] private object _content;
    [ObservableProperty] private string _title;
    [ObservableProperty]  private StreamGeometry? _icon;
    [ObservableProperty]  private bool _canClose;
    public bool IconIsVisible => Icon != null;

    public void Close()
    {
    }
}