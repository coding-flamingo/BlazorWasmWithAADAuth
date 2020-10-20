using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorWasmWithAADAuth.Client.Services
{
    public class DirectoryObjects
    {
        [JsonPropertyName("@odata.context")]
        public string Context { get; set; }

        [JsonPropertyName("value")]
        public List<Value> Values { get; set; }
    }

    public class Value
    {
        [JsonPropertyName("@odata.type")]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
