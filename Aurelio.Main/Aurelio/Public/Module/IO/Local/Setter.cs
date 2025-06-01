using System.IO;
using System.Linq;

namespace Aurelio.Public.Module.IO.Local;

public abstract class Setter
{
    public static void TryCreateFolder(string path)
    {
        if (Directory.Exists(path)) return;
        var directoryInfo = new DirectoryInfo(path);
        directoryInfo.Create();
    }

    public static void ClearFolder(string folderPath, string[]? ignore = null)
    {
        if (ignore != null && ignore.Contains(folderPath)) return;
        if (!Directory.Exists(folderPath))
        {
            return;
        }

        foreach (var file in Directory.GetFiles(folderPath))
        {
            File.Delete(file);
        }

        foreach (var dir in Directory.GetDirectories(folderPath))
        {
            ClearFolder(dir, ignore);
            Directory.Delete(dir);
        }
    }
}