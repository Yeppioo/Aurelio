using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Aurelio.Public.Classes.Entries;

namespace Aurelio.Public.Langs;

public sealed class LangHelper : INotifyPropertyChanged
{


    public static LangHelper Current { get; } = new();

    private MainLang _resources;

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

    private LangHelper()
    {
        _resources = new MainLang();
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void ChangedCulture(string? name)
    {
        MainLang.Culture = CultureInfo.GetCultureInfo(string.IsNullOrEmpty(name) ? "zh-CN" : name);
        Resources = new MainLang();
    }

    public string GetString(string key)
    {
        return MainLang.ResourceManager.GetString(key, MainLang.Culture) ?? $"[{key}]";
    }
}