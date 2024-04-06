using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchIrcClient.EventSub.Messages
{
    public class EventSubKeepalive : EventSubMessage
    {
        public override EventSubMessageType MessageType => EventSubMessageType.Keepalive;
        [JsonRequired]
        [JsonPropertyName("metadata")]
        public EventSubMessageBaseMetadata Metadata { get; set; }
        [JsonRequired]
        [JsonPropertyName("payload")]
        public JsonObject Payload { get; set; }
    }
}
