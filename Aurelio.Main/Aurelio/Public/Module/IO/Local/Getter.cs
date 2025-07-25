using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public static Bitmap LoadBitmapFromAppFile(string uri, int width = 48)
    {
        // return null;
        var memoryStream = new MemoryStream();
        var stream = AssetLoader.Open(new Uri("resm:" + uri));
        stream.CopyTo(memoryStream);
        memoryStream.Position = 0;
        return Bitmap.DecodeToWidth(memoryStream, width);
    }
    
    public static List<string> GetAllFilesByExtension(string folderPath, string fileExtension)
    {
        List<string> files = [];

        // 检查路径是否存在
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("指定的文件夹路径不存在！");
            return files;
        }

        // 使用递归方法获取所有文件
        var dirInfo = new DirectoryInfo(folderPath);
        files.AddRange(GetFilesRecursive(dirInfo, fileExtension));

        return files;

        static List<string> GetFilesRecursive(DirectoryInfo dirInfo, string fileExtension)
        {
            List<string> files = [];

            // 获取当前目录中的所有指定后缀的文件
            var fileInfos = dirInfo.GetFiles(fileExtension, SearchOption.TopDirectoryOnly);
            files.AddRange(fileInfos.Select(fileInfo => fileInfo.FullName));

            // 递归获取子目录中的文件
            var subDirs = dirInfo.GetDirectories();
            foreach (var subDir in subDirs)
            {
                files.AddRange(GetFilesRecursive(subDir, fileExtension));
            }

            return files;
        }
    }
}