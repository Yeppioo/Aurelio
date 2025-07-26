using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Aurelio.Public.Langs;

public sealed class GlobalLangHelper : INotifyPropertyChanged
{
    public static GlobalLangHelper Current { get; } = new();

    private CultureInfo _currentCulture;

    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        set
        {
            SetField(ref _currentCulture, value);
            CultureChanged.Invoke(this, value);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event CultureChangedEventHandler? CultureChanged;

    public delegate void CultureChangedEventHandler(object? sender, CultureInfo culture);

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }
    
    public void ChangedCulture(string? name)
    {
        CurrentCulture = CultureInfo.GetCultureInfo(string.IsNullOrEmpty(name) ? "zh-CN" : name);;
    }
}