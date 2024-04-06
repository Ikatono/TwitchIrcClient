using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchIrcClient.ApiClient.Messages
{
    public enum EventSubSubscriptionStatus
    {
        Enabled = 0,
        WebhookCallbackVerificationPending = 1,
        WebhookCallbackVerificationFailed = 2,
        NotificationFailuresExceeded = 3,
        AuthorizationRevoked = 4,
        ModeratorRemoved = 5,
        UserRemoved = 6,
        VersionRemoved = 7,
        BetaMaintenance = 8,
        WebsocketDisconnected = 9,
        WebsocketFailedPingPong = 10,
        WebsocketReceivedInboundTraffic = 11,
        WebsocketConnectionUnused = 12,
        WebsocketInternalError = 13,
        WebsocketNetworkTimeout = 14,
        WebsocketNetworkError = 15,
    }
    public class EventSubSubscriptionStatusConverter : JsonConverter<EventSubSubscriptionStatus>
    {
        private static readonly IList<KeyValuePair<EventSubSubscriptionStatus, string>> ConversionList =
        [
            new (EventSubSubscriptionStatus.Enabled, "enabled"),
            new (EventSubSubscriptionStatus.WebhookCallbackVerificationPending, "webhook_callback_verification_pending"),
            new (EventSubSubscriptionStatus.WebhookCallbackVerificationFailed, "webhook_callback_verification_failed"),
            new (EventSubSubscriptionStatus.NotificationFailuresExceeded, "notification_failures_exceeded"),
            new (EventSubSubscriptionStatus.AuthorizationRevoked, "authorization_revoked"),
            new (EventSubSubscriptionStatus.ModeratorRemoved, "moderator_removed"),
            new (EventSubSubscriptionStatus.UserRemoved, "user_removed"),
            new (EventSubSubscriptionStatus.VersionRemoved, "version_removed"),
            new (EventSubSubscriptionStatus.BetaMaintenance, "beta_maintenance"),
            new (EventSubSubscriptionStatus.WebsocketDisconnected, "websocket_disconnected"),
            new (EventSubSubscriptionStatus.WebsocketFailedPingPong, "websocket_failed_ping_pong"),
            new (EventSubSubscriptionStatus.WebsocketReceivedInboundTraffic, "websocket_received_inbound_traffic"),
            new (EventSubSubscriptionStatus.WebsocketConnectionUnused, "websocket_connection_unused"),
            new (EventSubSubscriptionStatus.WebsocketInternalError, "websocket_internal_error"),
            new (EventSubSubscriptionStatus.WebsocketNetworkTimeout, "websocket_network_timeout"),
            new (EventSubSubscriptionStatus.WebsocketNetworkError, "websocket_network_error"),
        ];
        private static readonly IList<KeyValuePair<string, EventSubSubscriptionStatus>> InverseConversionList =
            ConversionList.Select<KeyValuePair<EventSubSubscriptionStatus, string>,
                KeyValuePair<string, EventSubSubscriptionStatus>>(p => new(p.Value, p.Key)).ToList();

        public static string Convert(EventSubSubscriptionStatus status)
            => ConversionList.First(p => p.Key == status).Value;
        public static EventSubSubscriptionStatus Convert(string status)
            => InverseConversionList.First(p => p.Key == status).Value;
        public override EventSubSubscriptionStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => Convert(reader.GetString());

        public override void Write(Utf8JsonWriter writer, EventSubSubscriptionStatus value, JsonSerializerOptions options)
            => writer.WriteStringValue(Convert(value));
    }
}
