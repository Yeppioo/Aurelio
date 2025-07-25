namespace Aurelio.Plugin.Base;

public class RequirePluginEntry
{
    public string Id { get; set; }
    public Version[] VersionRange { get; set; } // [0] Min or Equal, [1] Max
    public RequireMethod RequireMethod { get; set; }
}

public enum RequireMethod
{ 
    Equal,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual
}