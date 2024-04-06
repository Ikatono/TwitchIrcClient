using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchIrcClient.ApiClient.Messages
{
    public record class EventSubResponseItem
    {
        [JsonRequired]
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonRequired]
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonRequired]
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonRequired]
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonRequired]
        [JsonPropertyName("condition")]
        public Dictionary<string, string> Condition { get; set; }
        [JsonRequired]
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonRequired]
        [JsonPropertyName("transport")]
        public Dictionary<string, string> Transport { get; set; }
        [JsonRequired]
        [JsonPropertyName("cost")]
        public int Cost { get; set; }
    }
    public record class EventSubResponse
    {
        [JsonRequired]
        [JsonPropertyName("data")]
        public EventSubResponseItem[] Data { get; set; }
        [JsonRequired]
        [JsonPropertyName("total")]
        public int Total { get; set; }
        [JsonRequired]
        [JsonPropertyName("total_cost")]
        public int TotalCost { get; set; }
        [JsonRequired]
        [JsonPropertyName("max_total_cost")]
        public int MaxTotalCost { get; set; }
    }
}
