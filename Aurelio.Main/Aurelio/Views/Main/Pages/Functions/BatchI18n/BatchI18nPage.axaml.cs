using System;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Page;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui;
using Aurelio.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Material.Icons;

namespace Aurelio.Views.Main.Pages.Functions.BatchI18n;

public partial class BatchI18nPage : PageMixModelBase, IFunctionPage
{
    private string _sourceFile;
    public TabEntry HostTab { get; set; }

    public BatchI18nPage()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += LoadedHandler;
    }

    public PageInfoEntry PageInfo => new()
    {
        Title = MainLang.BatchI18n,
        Icon = Icon.FromMaterial(MaterialIconKind.Translate)
    };

    public string SourceFile
    {
        get => _sourceFile;
        set => SetField(ref _sourceFile, value);
    }
    
    private async void LoadedHandler(object? sender, RoutedEventArgs e)
    {
        try
        {
            var files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                AllowMultiple = false,
                Title = MainLang.OpenLangSourceFile
            });
            if (files.Count == 0)
            {
                HostTab.Close();
                return;
            }

            SourceFile = files[0].Path.LocalPath;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    public void OnClose()
    {
        Loaded -= LoadedHandler;
        DataContext = null;
    }
}