using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Entries;

public class AccountRecordEntry(string account, string password) : ReactiveObject
{
    [Reactive] [JsonProperty] public string Account { get; set; } = account;
    [Reactive] [JsonProperty]  public string Password { get; set; } = password;

    public override bool Equals(object? obj)
    {
        if (obj is not AccountRecordEntry entry) return false;
        return Account == entry.Account && Password == entry.Password;
    }
}