using Aurelio.Public.Classes.Interfaces;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using Ursa.Controls;

namespace Aurelio.Views.Main;

public partial class AurelioWindow : UrsaWindow
{
    private readonly IExpandablePage _content;

    public AurelioWindow()
    {
        InitializeComponent();
    }

    public AurelioWindow(IExpandablePage content, string title)
    {
        _content = content;
        InitializeComponent();
        Frame.DataContext = ((Control)_content).DataContext;
        Frame.Content = content;
        TitleText.Text = title;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        _content.OnClose();
        base.OnClosing(e);
    }
}