namespace Aurelio.Public.Classes.Entries;

public class NavPageEntry(AurelioStaticPageInfo staticPageInfo, Func<(object sender, object? args), object>? action)
{
    public AurelioStaticPageInfo StaticPageInfo { get; set; } = staticPageInfo;
    public readonly Func<(object sender, object? args), object>? Create = action;
}