using System;
using Aurelio.Public.Classes.Entries;
using Avalonia.Controls;
using Avalonia.Media;

namespace Aurelio.Public.Classes.Interfaces;

public interface IFunctionPage
{
    public (string title, StreamGeometry icon, Action OnClose) GetPageInfo();
    public TabEntry HostTab { get; set; }
    public UserControl HostContent { get; set; }
    public void OnClose();
}