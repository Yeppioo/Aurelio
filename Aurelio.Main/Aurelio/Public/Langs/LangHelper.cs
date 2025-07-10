using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Aurelio.Public.Langs;

public sealed class LangHelper : INotifyPropertyChanged
{
    private MainLang _resources;

    private LangHelper()
    {
        _resources = new MainLang();
    }


    public static LangHelper Current { get; } = new();

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