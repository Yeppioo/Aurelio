using System;

namespace Aurelio.Public.Const;

public abstract class HttpClient
{
    public static System.Net.Http.HttpClient AurelioApiClient { get; set; } = new()
    {
        BaseAddress = new Uri(Config.AurelioApi),
        Timeout = TimeSpan.FromSeconds(30)
    };

    public static void InitClient()
    {
        AurelioApiClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }
}