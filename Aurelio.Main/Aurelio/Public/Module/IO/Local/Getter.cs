using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

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

    public static Bitmap LoadBitmapFromAppFile(string uri)
    {
        // return null;
        var memoryStream = new MemoryStream();
        var stream = AssetLoader.Open(new Uri("resm:" + uri));
        stream.CopyTo(memoryStream);
        memoryStream.Position = 0;
        return Bitmap.DecodeToWidth(memoryStream, 48);
    }
}