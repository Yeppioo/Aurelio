namespace Aurelio.Public.Classes.Entries;

public class Language
{
    public string Code { get; set; }
    public string Label { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Language other) return false;
        if (other.Code == "" && Code == "zh-CN") return true;
        if (other.Code == "zh-CN" && Code == "") return true;
        return Code == other.Code;
    }
}