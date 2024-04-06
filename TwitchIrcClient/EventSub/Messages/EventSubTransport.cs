using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchIrcClient.EventSub.Messages
{
    public record class EventSubTransport
    {
        /// <summary>
        /// The transport method. Possible values are:
        /// webhook
        /// websocket
        /// </summary>
        [JsonRequired]
        [JsonConverter(typeof(TwitchTransportTypeConverter))]
        [JsonPropertyName("method")]
        public TwitchTransportType Method { get; set; }
        /// <summary>
        /// The callback URL where the notifications are sent. The URL must use the HTTPS protocol and port 443.
        /// See <see href="https://dev.twitch.tv/docs/eventsub/handling-webhook-events#processing-an-event"/>.
        /// Specify this field only if <see cref="Method"/> is set to <see cref="TwitchTransportType.Webhook"/>.
        /// NOTE: Redirects are not followed.
        /// </summary>
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
        [JsonPropertyName("connected_at")]
        public DateTime? ConnectedAt { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("disconnected_at")]
        public DateTime? DisconnectedAt { get; set; }

        public EventSubTransport()
        {

        }
        private EventSubTransport(TwitchTransportType method,
            string? callback, string? secret, string? sessionId,
            DateTime? connectedAt, DateTime? disconnectedAt)
        {
            Method = method;
            Callback = callback;
            Secret = secret;
            SessionId = sessionId;
            ConnectedAt = connectedAt;
            DisconnectedAt = disconnectedAt;
        }
        public static EventSubTransport MakeWebhook(string callback, string secret)
            => new EventSubTransport(TwitchTransportType.Webhook,
                callback, secret, null, null, null);
        public static EventSubTransport MakeWebsocket(string sessionId, DateTime? connectedAt,
            DateTime? disconnectedAt) => new EventSubTransport(TwitchTransportType.Websocket,
                null, null, sessionId, connectedAt, disconnectedAt);
    }
    public enum TwitchTransportType
    {
        Webhook = 0,
        Websocket = 1,
        Conduit = 2,
    }
    public class TwitchTransportTypeConverter : JsonConverter<TwitchTransportType>
    {
        public override TwitchTransportType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.GetString() switch
            {
                "webhook" => TwitchTransportType.Webhook,
                "websocket" => TwitchTransportType.Websocket,
                "conduit" => TwitchTransportType.Conduit,
                _ => throw new JsonException(),
            };
        public override void Write(Utf8JsonWriter writer, TwitchTransportType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value switch
            {
                TwitchTransportType.Webhook => "webhook",
                TwitchTransportType.Websocket => "websocket",
                TwitchTransportType.Conduit => "conduit",
                _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(TwitchTransportType)),
            });
        }
    }
}
