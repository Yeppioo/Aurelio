using System.ComponentModel;
using Aurelio.Public.Classes.Entries;
using Avalonia.Media;

namespace Aurelio.Public.Classes.Interfaces;

public interface IFunctionPage : INotifyPropertyChanged
{
    public (string title, StreamGeometry icon) GetPageInfo();
    public TabEntry HostTab { get; set; }
    public void OnClose();
}