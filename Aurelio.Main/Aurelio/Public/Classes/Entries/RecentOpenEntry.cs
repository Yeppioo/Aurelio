using System;
using System.IO;
using System.Linq;
using Aurelio.Public.Const;
using Aurelio.Public.Enum;
using Aurelio.Public.Module;
using Aurelio.Views.Main.Pages;
using DynamicData;

namespace Aurelio.Public.Classes.Entries;

public class RecentOpenEntry
{
    public FunctionType FunctionType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public object? Data { get; set; }
    public DateTime LastTime { get; set; } = DateTime.Now;
    
    public override bool Equals(object? obj)
    {
        if (obj is not RecentOpenEntry entry) return false;
        return entry.FunctionType == FunctionType && entry.FilePath == FilePath && entry.Title == Title && entry.Summary == Summary;
    }

    public void Remove()
    {
        UiProperty.RecentOpens.Remove(this);
        (App.UiRoot.ViewModel.Tabs.First().Content as HomePage)?.FilterRecentOpens();
        File.WriteAllText(ConfigPath.RecentOpenDataPath, UiProperty.RecentOpens.AsJson());
    }
}