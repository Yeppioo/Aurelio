namespace Aurelio.Public.Classes.Entries;

public class UpdateInfo
{
    public string NewVersion { get; set; }
    public bool IsNeedUpdate { get; set; }
    public DateTime ReleaseTime { get; set; }
    public string Body { get; set; }
}