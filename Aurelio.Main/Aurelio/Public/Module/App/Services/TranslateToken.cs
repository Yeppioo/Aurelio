using System.Net.Http;
using Aurelio.Public.Module.IO;

namespace Aurelio.Public.Module.App.Services;

public class TranslateToken
{
    public static async Task RefreshToken()
    {
        await Task.Delay(200);
        while (true)
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                };
                using var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/18.17763");
                client.DefaultRequestHeaders.Add("Accept", "*/*");
                client.DefaultRequestHeaders.Add("Host", "edge.microsoft.com");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");

                var response =
                    await client.GetAsync("https://edge.microsoft.com/translate/auth");
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                Data.TranslateToken = responseBody;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            await Task.Delay(TimeSpan.FromMinutes(4));
        }
    }
}