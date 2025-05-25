using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Aurelio.Public.Module.IO.Http.Api.Aurelio;

public abstract class Login : ApiBase
{
    public static async Task<(int code, LoginData.Root? data)> Send(string? account, string? password)
    {
        var data = await PostAsync($"/api/user/login", new { account, password });
        var result = JsonConvert.DeserializeObject<LoginData.Root>(data.message);
        return (data.code, result);
    }

    public class LoginData
    {
        public class Data
        {
            public string email { get; set; }
            public string username { get; set; }
            public string avatarUrl { get; set; }
        }

        public class Root
        {
            public int code { get; set; }
            public string message { get; set; }
            public Data data { get; set; }
        }
    }
}