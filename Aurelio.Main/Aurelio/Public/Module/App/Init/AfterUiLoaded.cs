using System.Diagnostics;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Const;

namespace Aurelio.Public.Module.App.Init;

public abstract class AfterUiLoaded
{
    public static void Main()
    {
        Data.ProjectIndexEntries.Add(new ProjectIndexEntry());
    }
}