using System.ComponentModel;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Functions;
using Avalonia.Media;

namespace Aurelio.Public.Classes.Interfaces;

public interface IFunctionPage : INotifyPropertyChanged
{
    public TabEntry HostTab { get; set; }
    public void OnClose();
    public PageInfoEntry PageInfo { get; }
}