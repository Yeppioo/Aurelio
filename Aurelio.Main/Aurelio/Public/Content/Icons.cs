using Avalonia.Media;

namespace Aurelio.Public.Content;

public class Icons
{
    public static readonly StreamGeometry WindowMaximize = Parse("F1 M8.66 10.59L1.93 10.59C0.86 10.59 0 9.72 0 8.66L0 1.93C0 0.87 0.87 0 1.93 0L8.66 0C9.73 0 10.59 0.87 10.59 1.93L10.59 8.66C10.59 9.73 9.72 10.59 8.66 10.59ZM1.93 1.4C1.64 1.4 1.4 1.64 1.4 1.93L1.4 8.66C1.4 8.95 1.64 9.19 1.93 9.19L8.66 9.19C8.95 9.19 9.19 8.95 9.19 8.66L9.19 1.93C9.19 1.64 8.95 1.4 8.66 1.4L1.93 1.4Z");
    private static StreamGeometry Parse(string path) => StreamGeometry.Parse(path);

}