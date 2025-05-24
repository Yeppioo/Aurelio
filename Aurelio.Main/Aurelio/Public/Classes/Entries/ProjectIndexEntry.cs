using System;

namespace Aurelio.Public.Classes.Entries;

public class ProjectIndexEntry
{
    public string Title { get; set; } = string.Empty;
    public string UUid { get; set; } = Guid.NewGuid().ToString();
}