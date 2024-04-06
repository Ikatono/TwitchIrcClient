using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchIrcClient.ApiClient.Messages
{
    public record class EventSubRequest
    {
        [JsonRequired]
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonRequired]
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonRequired]
        [JsonPropertyName("condition")]
        public Dictionary<string, string> Condition;
        [JsonRequired]
        [JsonPropertyName("transport")]
        public ApiTransport Transport { get; set; }

        [JsonConstructor]
        public EventSubRequest(string type, string version,
            IEnumerable<KeyValuePair<string, string>> condition,
            ApiTransport transport)
        {
            Type = type;
            Version = version;
            Condition = condition.ToDictionary();
            Transport = transport;
        }
    }
}
