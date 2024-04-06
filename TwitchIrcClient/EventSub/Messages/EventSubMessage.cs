using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace TwitchIrcClient.EventSub.Messages
{
    public class EventSubMessageBaseMetadata
    {
        [JsonRequired]
        [JsonPropertyName("message_id")]
        public string MessageId { get; set; }
        [JsonRequired]
        [JsonPropertyName("message_type")]
        public string MessageType { get; set; }
        [JsonRequired]
        [JsonPropertyName("message_timestamp")]
        public DateTime MessageTime { get; set; }
    }
    public abstract class EventSubMessage
    {
        [JsonIgnore]
        public abstract EventSubMessageType MessageType { get; }
        public static EventSubMessage Parse(string json)
        {
            var node = JsonNode.Parse(json);
            if (!(node?["metadata"]?["message_type"]?.AsValue().TryGetValue(out string? value) ?? false))
                throw new ArgumentException("invalid json", nameof(json));
            return value switch
            {
                "session_welcome"   => JsonSerializer.Deserialize<EventSubWelcome>(node),
                "session_keepalive" => JsonSerializer.Deserialize<EventSubKeepalive>(node),
                "notification"      => JsonSerializer.Deserialize<EventSubNotification>(node),
                "session_reconnect" => JsonSerializer.Deserialize<EventSubReconnect>(node),
                "revocation"        => JsonSerializer.Deserialize<EventSubRevocation>(node),
                _                   => (EventSubMessage?)null,
            } ?? throw new ArgumentException("invalid json", nameof(json));
        }
    }
    public class EventSubNotificationEventArgs(EventSubNotification notification) : EventArgs
    {
        public EventSubNotification Notification = notification;
    }
    public class EventSubKeepaliveEventArgs(EventSubKeepalive keepalive) : EventArgs
    {
        public EventSubKeepalive Keepalive = keepalive;
    }
    public class EventSubReconnectEventArgs(EventSubReconnect reconnect) : EventArgs
    {
        public EventSubReconnect Reconnect = reconnect;
    }
    public class EventSubRevocationEventArgs(EventSubRevocation revocation) : EventArgs
    {
        public EventSubRevocation Revocation = revocation;
    }
    public class EventSubWelcomeEventArgs(EventSubWelcome welcome) : EventArgs
    {
        public EventSubWelcome Welcome = welcome;
    }
}
