using System.ComponentModel;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Page;
using Avalonia.Media;

namespace Aurelio.Public.Classes.Interfaces;

public interface IFunctionPage : INotifyPropertyChanged
{
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }
    public void OnClose();
}