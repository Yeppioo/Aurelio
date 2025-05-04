using System;
using System.IO;
using Avalonia.Media.Imaging;

namespace Aurelio.Public.Module.Value;

public class Converter
{
    public static Bitmap? Base64ToBitmap(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            return null;
        }

        var imageBytes = Convert.FromBase64String(base64);
        using var ms = new MemoryStream(imageBytes);
        var bitmap = new Bitmap(ms);
        return bitmap;
    }
}