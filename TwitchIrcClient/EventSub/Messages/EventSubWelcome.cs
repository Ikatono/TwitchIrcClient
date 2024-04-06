using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchIrcClient.EventSub.Messages
{
    public class EventSubWelcome : EventSubMessage
    {
        public override EventSubMessageType MessageType => EventSubMessageType.Welcome;
        [JsonRequired]
        [JsonPropertyName("metadata")]
        public EventSubMessageBaseMetadata Metadata { get; set; }
        [JsonRequired]
        [JsonPropertyName("payload")]
        public EventSubReconnectPayload Payload { get; set; }
    }
}
