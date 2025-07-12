using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Entries;

public class PageInfoEntry : ReactiveObject
{
    [Reactive] public string Title { get; set; }
    [Reactive] public StreamGeometry Icon { get; init; }
    [Reactive] public bool CanClose { get; init; } = true;
}