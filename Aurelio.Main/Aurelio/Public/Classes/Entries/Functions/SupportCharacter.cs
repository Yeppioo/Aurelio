using System.Collections.Generic;
using System.Unicode;

namespace Aurelio.Public.Classes.Entries.Functions;

public class SupportCharacter
{
    public string Name { get; set; } = string.Empty;
    public List<CharacterBlock> CharacterBlocks { get; init; } = [];
}

public class CharacterBlock
{
    public string Name { get; set; } = string.Empty;
    public List<CharacterEntry> Characters { get; set; } = [];
}

public record CharacterEntry
{
    public string Char { get; set; }
    public int Code { get; init; }
    public string Hex => $"U+{Code:X4}";
    public string Name{ get; init; }
}