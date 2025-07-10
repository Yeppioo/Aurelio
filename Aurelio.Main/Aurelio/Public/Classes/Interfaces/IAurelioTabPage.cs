using Aurelio.Public.Classes.Entries;

namespace Aurelio.Public.Classes.Interfaces;

public interface IAurelioTabPage : IAurelioPage
{
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }
    public void OnClose();
}