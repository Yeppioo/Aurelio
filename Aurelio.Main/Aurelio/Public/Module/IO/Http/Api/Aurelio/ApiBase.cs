using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Aurelio.Public.Const;
using Newtonsoft.Json;
using HttpClient = Aurelio.Public.Const.HttpClient;

namespace Aurelio.Public.Module.IO.Http.Api.Aurelio;

public class ApiBase
{
    protected static async Task<(int code, string message)> PostAsync(string url, object requestData)
    {
        var json = requestData.ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await HttpClient.AurelioApiClient.PostAsync(url, content);
        return ((int)response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}