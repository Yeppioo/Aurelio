using Aurelio.Public.Enum;

namespace Aurelio.Public.Classes.Entries;

public class RecentOpen
{
    public FunctionType FunctionType { get; set; }
    public string? FilePath { get; set; }
    public string? Data { get; set; }
}