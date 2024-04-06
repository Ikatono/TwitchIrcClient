using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchIrcClient.EventSub.Messages
{
    public class EventSubNotificationMetadata : EventSubMessageBaseMetadata
    {
        [JsonRequired]
        [JsonPropertyName("subscription_type")]
        public string SubscriptionType { get; set; }
        [JsonRequired]
        [JsonPropertyName("subscription_version")]
        public string SubscriptionVersion { get; set; }
    }
    public class EventSubNotificationTransport
    {
        [JsonRequired]
        [JsonPropertyName("method")]
        public string Method { get; set; }
        [JsonRequired]
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; }
    }
    public class EventSubNotificationSubscription
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
        [JsonPropertyName("cost")]
        public int Cost { get; set; }
        [JsonRequired]
        [JsonPropertyName("condition")]
        public object Condition { get; set; }
        [JsonRequired]
        [JsonPropertyName("transport")]
        public EventSubNotificationTransport Transport { get; set; }
        [JsonRequired]
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
    public class EventSubNotificationPayload
    {
        [JsonRequired]
        [JsonPropertyName("subscription")]
        public EventSubNotificationSubscription Subscription { get; set; }
        [JsonRequired]
        [JsonPropertyName("event")]
        public JsonObject Event { get; set; }
    }
    public class EventSubNotification : EventSubMessage
    {
        [JsonIgnore]
        public override EventSubMessageType MessageType => EventSubMessageType.Notification;
        [JsonRequired]
        [JsonPropertyName("metadata")]
        public EventSubNotificationMetadata Metadata { get; set; }
        [JsonRequired]
        [JsonPropertyName("payload")]
        public EventSubNotificationPayload Payload { get; set; }
    }
}
