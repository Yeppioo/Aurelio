using System.IO;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Langs;
using Aurelio.Views.Main;
using Aurelio.Views.Main.Template;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;

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
        Image.Source = Bitmap.DecodeToHeight(new MemoryStream(File.ReadAllBytes(path)), 135);
        Root.PointerReleased += (_, _) =>
        {
            var tab = new TabEntry(new ImageViewer(name, new Bitmap(path), path));
            if (this.GetVisualRoot() is TabWindow window)
            {
                window.CreateTab(tab);
                return;
            }
            App.UiRoot.CreateTab(tab);
        };
    }
}