using Newtonsoft.Json;

namespace Aurelio.Public.Module;

public static class Extensions
{
    public static string AsJson(this object obj, Formatting formatting = Formatting.Indented)
    {
        return JsonConvert.SerializeObject(obj, formatting);
    }
    
    public static bool IsNullOrWhiteSpace(this string str)
    {
        return string.IsNullOrWhiteSpace(str);
    }
}