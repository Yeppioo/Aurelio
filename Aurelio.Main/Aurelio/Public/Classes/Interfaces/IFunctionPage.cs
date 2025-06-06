using System;
using System.ComponentModel;
using Aurelio.Public.Classes.Entries;
using Avalonia.Controls;
using Avalonia.Media;

namespace Aurelio.Public.Classes.Types;

public interface IFunctionPage : IDisposable , INotifyPropertyChanged
{
    public (string title, StreamGeometry icon) GetPageInfo();
    public TabEntry HostTab { get; set; }
}