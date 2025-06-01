using System.Collections.Generic;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Enum;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui;
using Material.Icons;

namespace Aurelio.ViewModels;

public abstract class NewPageConfig
{
    public static List<NewPageEntry> NewPageEntries { get; } =
    [
        new(FunctionType.CharacterMapping, MainLang.CharacterMapping, Icons.CharacterAppearance),
    ];
}