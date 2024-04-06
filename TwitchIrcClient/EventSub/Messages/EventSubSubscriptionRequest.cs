using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchIrcClient.EventSub.Messages
{
    //this will fail for "Channel Moderate Event" because of the complicated "Condition" field
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public record class EventSubSubscriptionRequest
    {
        [JsonRequired]
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonRequired]
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonRequired]
        [JsonPropertyName("condition")]
        public Dictionary<string,string> Condition { get; set; }
        [JsonRequired]
        [JsonPropertyName("transport")]
        public Dictionary<string, string> Transport { get; set; }
        public EventSubSubscriptionRequest(string type, string version,
            Dictionary<string, string> condition,
            Dictionary<string, string> transport)
        {
            Type = type;
            Version = version;
            Condition = condition;
            Transport = transport;
        }
    }
}
