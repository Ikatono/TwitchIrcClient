using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchIrcClient.EventSub.Messages
{
    public class EventSubReconnectPayloadSession
    {
        [JsonRequired]
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonRequired]
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonRequired]
        [JsonPropertyName("keepalive_timeout_seconds")]
        public int? KeepaliveTimeoutSeconds { get; set; }
        [JsonRequired]
        [JsonPropertyName("reconnect_url")]
        public string ReconnectUrl { get; set; }
        [JsonRequired]
        [JsonPropertyName("connected_at")]
        public DateTime ConnectedAt { get; set; }
    }
    public class EventSubReconnectPayload
    {
        [JsonRequired]
        [JsonPropertyName("session")]
        public EventSubReconnectPayloadSession Session { get; set; }
    }
    public class EventSubReconnect : EventSubMessage
    {
        public override EventSubMessageType MessageType => EventSubMessageType.Reconnect;
        [JsonRequired]
        [JsonPropertyName("metadata")]
        public EventSubMessageBaseMetadata Metadata { get; set; }
        [JsonRequired]
        [JsonPropertyName("payload")]
        public EventSubReconnectPayload Payload { get; set; }
    }
}
