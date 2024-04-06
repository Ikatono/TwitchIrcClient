using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchIrcClient.EventSub.Messages
{
    public class EventSubRevocationPayload
    {
        [JsonRequired]
        [JsonPropertyName("subscription")]
        public EventSubNotificationSubscription Subscription { get; set; }
    }
    public class EventSubRevocation : EventSubMessage
    {
        public override EventSubMessageType MessageType => EventSubMessageType.Revocation;
        [JsonRequired]
        [JsonPropertyName("metadata")]
        public EventSubNotificationMetadata Metadata { get; set; }
        [JsonRequired]
        [JsonPropertyName("payload")]
        public EventSubRevocationPayload Payload { get; set; }
    }
}
