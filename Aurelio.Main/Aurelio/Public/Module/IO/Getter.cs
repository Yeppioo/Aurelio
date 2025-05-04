using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Aurelio.Public.Module.IO;

public class Getter
{
    public static List<string> GetAllFilesByExtension(string folderPath, string fileExtension)
    {
        List<string> files = [];

        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("指定的文件夹路径不存在！");
            return files;
        }

        var dirInfo = new DirectoryInfo(folderPath);
        files.AddRange(GetFilesRecursive(dirInfo, fileExtension));

        return files;

        static List<string> GetFilesRecursive(DirectoryInfo dirInfo, string fileExtension)
        {
            List<string> files = [];

            var fileInfos = dirInfo.GetFiles(fileExtension, SearchOption.TopDirectoryOnly);
            files.AddRange(fileInfos.Select(fileInfo => fileInfo.FullName));

            var subDirs = dirInfo.GetDirectories();
            foreach (var subDir in subDirs)
            {
                files.AddRange(GetFilesRecursive(subDir, fileExtension));
            }

            return files;
        }
    }
    
    public static Bitmap LoadBitmapFromAppFile(string uri)
    {
        var memoryStream = new MemoryStream();
        var stream = AssetLoader.Open(new Uri("resm:" + uri));
        stream.CopyTo(memoryStream);
        memoryStream.Position = 0;
        return new Bitmap(memoryStream);
    }
}