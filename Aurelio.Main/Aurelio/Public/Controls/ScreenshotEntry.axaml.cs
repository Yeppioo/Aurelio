using System.IO;
using Aurelio.Public.Classes.Entries;
using Aurelio.Views.Main;
using Aurelio.Views.Main.Template;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;

namespace Aurelio.Public.Controls;

public partial class ScreenshotEntry : UserControl
{
    private readonly string _path;
    private readonly Action _refreshAction;

    public ScreenshotEntry(string name, string path, Action refreshAction)
    {
        _path = path;
        _refreshAction = refreshAction;
        InitializeComponent();
        FileNameTextBlock.Text = name;
        var memoryStream = new MemoryStream(File.ReadAllBytes(path));
        Image.Source = Bitmap.DecodeToHeight(memoryStream, 135);
        Root.PointerReleased += (_, _) =>
        {
            var memoryStream1 = new MemoryStream(File.ReadAllBytes(path));
            var tab = new TabEntry(new ImageViewer(name, Bitmap.DecodeToWidth(memoryStream1,1080), path));
            if (this.GetVisualRoot() is TabWindow window)
            {
                window.CreateTab(tab);
                return;
            }

            App.UiRoot.CreateTab(tab);
        };
    }
}