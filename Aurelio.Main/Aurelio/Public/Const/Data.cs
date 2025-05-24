using System.Collections.ObjectModel;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Enum;

namespace Aurelio.Public.Const;

public class Data
{
    private static Data? _instance;
    public static Data Instance
    {
        get { return _instance ??= new Data(); }
    }
    public static DesktopType DesktopType { get; set; } = DesktopType.Unknown;
    public static ObservableCollection<ProjectIndexEntry> ProjectIndexEntries { get; set; } = [];
}