using System.Collections.ObjectModel;
using Aurelio.Public.Classes.Entries;

namespace Aurelio.Public.Langs;

public static class LanguageTypes
{
    public static ObservableCollection<Language> Langs { get; } =
    [
        new() { Label = "简体中文", Code = "zh-CN" },
        new() { Label = "繁體中文", Code = "zh-Hant" },
        new() { Label = "English", Code = "en" },
    ];
}