using System.ComponentModel;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Page;

namespace Aurelio.Public.Classes.Interfaces;

public interface IAurelioPage: INotifyPropertyChanged
{
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }
    public void OnClose();
}