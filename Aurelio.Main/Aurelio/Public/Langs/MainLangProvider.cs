using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Aurelio.Public.Langs;

public sealed class MainLangProvider : INotifyPropertyChanged
{
    private MainLang _resources;

    private MainLangProvider()
    {
        _resources = new MainLang();
        
        GlobalLangHelper.Current.CultureChanged += (_, e) =>
        {
            ChangedCulture(e);
        };
    }


    public static MainLangProvider Current { get; } = new();

    public MainLang Resources
    {
        get => _resources;
        private set
        {
            if (_resources == value) return;
            _resources = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void ChangedCulture(CultureInfo culture)
    {
        MainLang.Culture = culture;
        Resources = new MainLang();
    }
}