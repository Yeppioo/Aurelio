using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Aurelio.Public.Classes.Setting;
using Aurelio.Public.Module.IO;
using Newtonsoft.Json;

namespace Aurelio.Public.Module.App.Init.Config;

public abstract class Reader
{
    public static List<object> FailedSettingKeys { get; } = [];

    public static void Main()
    {
        try
        {
            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    FailedSettingKeys.Add(args);
                    args.ErrorContext.Handled = true;
                },
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            Data.SettingEntry = JsonConvert.DeserializeObject<SettingEntry>(
                File.ReadAllText(ConfigPath.SettingDataPath), settings
            ) ?? new SettingEntry();
        }
        catch (Exception ex)
        {
            FailedSettingKeys.Add($"Setting completely load failed: {ex.Message}");
            Data.SettingEntry = new SettingEntry();
        }

        if (FailedSettingKeys.Count > 0) Logger.Error($"Setting load with errors: {FailedSettingKeys.AsJson()}");

        GetVersion();
    }

    public static void GetVersion()
    {
        const string resourceName = "Aurelio.Version.txt";
        var _assembly = Assembly.GetExecutingAssembly();
        var stream = _assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!);
        var result = reader.ReadToEnd();
        Data.Instance.Version = $"v{result.Trim()}";
    }
}