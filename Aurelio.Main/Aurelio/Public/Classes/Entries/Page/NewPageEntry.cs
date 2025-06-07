using Aurelio.Public.Enum;
using Avalonia.Media;

namespace Aurelio.Public.Classes.Entries.Page;

public class NewPageEntry(FunctionType FunctionType, string Title, StreamGeometry Icon)
{
    public FunctionType FunctionType { get; set; } = FunctionType;
    public string Title { get; set; } = Title;
    public StreamGeometry Icon { get; set; } = Icon;
}