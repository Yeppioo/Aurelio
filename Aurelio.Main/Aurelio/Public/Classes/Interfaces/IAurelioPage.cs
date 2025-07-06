using System.ComponentModel;
using Aurelio.Public.Module.Ui.Helper;
using Avalonia.Controls;

namespace Aurelio.Public.Classes.Interfaces;

public interface IAurelioPage : INotifyPropertyChanged
{
    public Border RootElement { get; set; }
}