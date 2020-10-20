using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazorWasmWithAADAuth.Client.Services
{
    public class CustomUserAccount : RemoteUserAccount
    {
        [JsonPropertyName("groups")]
        public string[] Groups { get; set; } = new string[] { };

        [JsonPropertyName("roles")]
        public string[] Roles { get; set; } = new string[] { };
    }
}
