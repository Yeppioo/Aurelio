using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;
using AvaloniaEdit.Indentation;
using AvaloniaEdit.Indentation.CSharp;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using TextMateSharp.Grammars;

namespace Aurelio.Views.Main.Pages.Viewers;

public partial class CodeViewer : PageMixModelBase, IAurelioTabPage, IAurelioNavPage, IAurelioRequestableClosePage
{
    private string _title;
    private string _path;
    private bool _isWordWrapEnabled = true;
    private bool _isEditorModfied = false;

    public bool IsWordWrapEnabled
    {
        get => _isWordWrapEnabled;
        set => SetField(ref _isWordWrapEnabled, value);
    }

    public bool IsEditorModfied
    {
        get => _isEditorModfied;
        set
        {
            SetField(ref _isEditorModfied, value);
            UpdateTitle();
        }
    }

    private void UpdateTitle()
    {
        if (IsEditorModfied)
        {
            PageInfo.Title = "* " + _title;
        }
        else
        {
            PageInfo.Title = _title;
        }
    }

    public CodeViewer()
    {
    }

    public CodeViewer(string title, string path)
    {
        _title = title;
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
        try
        {
            _textMateInstallation.SetGrammar(
                _registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(Path.GetExtension(path))
                    .Id));
        }
        catch
        {
            // ignored
        }

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
        });KeyBindings.Add(new KeyBinding
        {
            Gesture = KeyGesture.Parse("Ctrl+R"),
            Command = new RelayCommand(Refresh)
        });
        Editor.TextChanged += (_, _) => IsEditorModfied = true;
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
        IsEditorModfied = false;
    }
    
    public void Refresh()
    {
        Editor.Document = new TextDocument(new StringTextSource(File.ReadAllText(_path)));
        IsEditorModfied = false;
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
        _path = f;
        _title = Path.GetFileName(_path);
        await File.WriteAllTextAsync(_path, Editor.Text);
        IsEditorModfied = false;
    }

    public void OnClose()
    {
    }

    public static IAurelioNavPage Create((object sender, object? param) t)
    {
        return new CodeViewer(Path.GetFileName((string)t.param!), (string)t.param!);
    }

    public static AurelioStaticPageInfo StaticPageInfo { get; } = new()
    {
        Icon = StreamGeometry.Parse(
            "M392.8 1.2c-17-4.9-34.7 5-39.6 22l-128 448c-4.9 17 5 34.7 22 39.6s34.7-5 39.6-22l128-448c4.9-17-5-34.7-22-39.6zm80.6 120.1c-12.5 12.5-12.5 32.8 0 45.3L562.7 256l-89.4 89.4c-12.5 12.5-12.5 32.8 0 45.3s32.8 12.5 45.3 0l112-112c12.5-12.5 12.5-32.8 0-45.3l-112-112c-12.5-12.5-32.8-12.5-45.3 0zm-306.7 0c-12.5-12.5-32.8-12.5-45.3 0l-112 112c-12.5 12.5-12.5 32.8 0 45.3l112 112c12.5 12.5 32.8 12.5 45.3 0s12.5-32.8 0-45.3L77.3 256l89.4-89.4c12.5-12.5 12.5-32.8 0-45.3z"),
        Title = "代码编辑器",
        NeedPath = true,
        AutoCreate = false,
        MustPath = true
    };

    public async Task<bool> RequestClose(object? sender)
    {
        if (!IsEditorModfied)
            return true;

        var cr = await ShowDialogAsync(
            $"是否要保存对 {_title} 的更改?",
            "如果不保存，你的更改将丢失。",
            b_primary: MainLang.Save,
            b_secondary: "不保存",
            b_cancel: MainLang.Cancel,
            sender: sender as Control ?? this);
        if (cr == ContentDialogResult.Primary)
        {
            Save();
            return true;
        }
        if (cr == ContentDialogResult.Secondary)
        {
            return true;
        }
        return false;
    }
}