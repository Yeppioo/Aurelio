using System.IO;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;
using CommunityToolkit.Mvvm.Input;
using TextMateSharp.Grammars;

namespace Aurelio.Views.Main.Pages.Viewers;

public partial class CodeViewer : PageMixModelBase, IAurelioTabPage
{
    private readonly string _path;
    private bool _isWordWrapEnabled = true;

    public bool IsWordWrapEnabled
    {
        get => _isWordWrapEnabled;
        set => SetField(ref _isWordWrapEnabled, value);
    }

    public CodeViewer()
    {
    }

    public CodeViewer(string title, string path)
    {
        _path = path;
        InitializeComponent();
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        PageInfo = new PageInfoEntry
        {
            Title = title,
            Icon = StreamGeometry.Parse(
                "M392.8 1.2c-17-4.9-34.7 5-39.6 22l-128 448c-4.9 17 5 34.7 22 39.6s34.7-5 39.6-22l128-448c4.9-17-5-34.7-22-39.6zm80.6 120.1c-12.5 12.5-12.5 32.8 0 45.3L562.7 256l-89.4 89.4c-12.5 12.5-12.5 32.8 0 45.3s32.8 12.5 45.3 0l112-112c12.5-12.5 12.5-32.8 0-45.3l-112-112c-12.5-12.5-32.8-12.5-45.3 0zm-306.7 0c-12.5-12.5-32.8-12.5-45.3 0l-112 112c-12.5 12.5-12.5 32.8 0 45.3l112 112c12.5 12.5 32.8 12.5 45.3 0s12.5-32.8 0-45.3L77.3 256l89.4-89.4c12.5-12.5 12.5-32.8 0-45.3z")
        };
        DataContext = this;
        Editor.Document = new TextDocument(new StringTextSource(File.ReadAllText(path)));
        var _registryOptions = new RegistryOptions(ThemeName.DarkPlus);
        var _textMateInstallation = Editor.InstallTextMate(_registryOptions);
        _textMateInstallation.SetGrammar(
            _registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(Path.GetExtension(path)).Id));
        Editor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Color.FromRgb(84, 169, 255));
        Editor.TextArea.SelectionBrush = new SolidColorBrush(Color.Parse("#3E3574F0"));

        KeyBindings.Add(new KeyBinding
        {
            Gesture = KeyGesture.Parse("Ctrl+Shift+S"),
            Command = new RelayCommand(SaveAs)
        });
        KeyBindings.Add(new KeyBinding
        {
            Gesture = KeyGesture.Parse("Ctrl+S"),
            Command = new RelayCommand(Save)
        });
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public void SelectAll()
    {
        Editor.SelectAll();
    }

    public void Copy()
    {
        Editor.Copy();
    }

    public void Cut()
    {
        Editor.Cut();
    }

    public void Paste()
    {
        Editor.Paste();
    }

    public void Redo()
    {
        Editor.Redo();
    }

    public void Undo()
    {
        Editor.Undo();
    }

    public void Save()
    {
        File.WriteAllText(_path, Editor.Text);
    }

    public async void SaveAs()
    {
        var f = await this.PickSaveFileAsync(new FilePickerSaveOptions
        {
            Title = MainLang.SaveAs,
            SuggestedFileName = Path.GetFileName(_path),
            FileTypeChoices =
            [
                new FilePickerFileType("File") { Patterns = [$"*{Path.GetExtension(_path)}"] }
            ]
        });
        if (string.IsNullOrWhiteSpace(f)) return;
        await File.WriteAllTextAsync(f, Editor.Text);
    }

    public void OnClose()
    {
    }
}