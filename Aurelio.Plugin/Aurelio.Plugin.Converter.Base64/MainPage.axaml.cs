using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aurelio.Plugin.Base64Converter;

public partial class MainPage : UserControl
{
    public MainPage()
    {
        InitializeComponent();
        ToBaseButton.Click += (_, _) =>
        {
            Base64Editor.Text = Convert.ToBase64String(Encoding.UTF8.GetBytes(StringEditor.Text));
        };
        ToStringButton.Click += (_, _) =>
        {
            StringEditor.Text = Encoding.UTF8.GetString(Convert.FromBase64String(Base64Editor.Text));
        };
    }
}