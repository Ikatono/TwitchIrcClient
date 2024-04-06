using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TwitchIrcClient.EventSub.Messages;

namespace TwitchIrcClient.ApiClient.Messages
{
    public record class ApiTransport
    {
        [JsonRequired]
        [JsonConverter(typeof(TwitchTransportTypeConverter))]
        [JsonPropertyName("method")]
        public TwitchTransportType Method { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("callback")]
        public string? Callback { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("secret")]
        public string? Secret { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("session_id")]
        public string? SessionId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("conduit_id")]
        public string? ConduitId { get; set; }

        [JsonConstructor]
        public ApiTransport(TwitchTransportType method)
        {
            Method = method;
        }

        public static ApiTransport MakeForWebhook(string callback, string secret)
            => new(TwitchTransportType.Webhook)
            {
                Callback = callback,
                Secret = secret,
            };
        public static ApiTransport MakeForWebsocket(string sessionId)
            => new(TwitchTransportType.Websocket)
            {
                SessionId = sessionId,
            };
        public static ApiTransport MakeForConduit(string conduitId)
            => new(TwitchTransportType.Conduit)
            {
                ConduitId = conduitId,
            };
    }
}
