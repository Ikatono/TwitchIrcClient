using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchIrcClient.ApiClient.Messages
{
    public record class EventSubSubscriptionListResponsePagination
    {
        [JsonRequired]
        [JsonPropertyName("cursor")]
        public string Cursor { get; set; }
    }
    public record class EventSubSubscriptionListResponse
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
        [JsonPropertyName("pagination")]
        public EventSubSubscriptionListResponsePagination? Pagination { get; set; }
    }
}
