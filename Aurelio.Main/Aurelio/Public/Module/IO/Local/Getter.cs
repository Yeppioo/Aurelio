using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Aurelio.Public.Module.IO.Local;

public class Getter
{
    public static async Task<string> ReadAllAppFileText(string uri)
    {
        var _assembly = Assembly.GetExecutingAssembly();
        var stream = _assembly.GetManifestResourceStream(uri);
        using var reader = new StreamReader(stream!);
        var result = await reader.ReadToEndAsync();
        return result;
    }
}