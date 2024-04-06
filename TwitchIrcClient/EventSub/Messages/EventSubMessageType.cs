using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIrcClient.EventSub.Messages
{
    public enum EventSubMessageType
    {
        Welcome      = 0,
        Keepalive    = 1,
        Notification = 2,
        Reconnect    = 3,
        Revocation   = 4,
    }
    internal static class EventSubMessageTypeHelper
    {
        public static EventSubMessageType Parse(string s)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(s);
            return s.ToLower() switch
            {
                "session_welcome"   => EventSubMessageType.Welcome,
                "session_keepalive" => EventSubMessageType.Keepalive,
                "notification"      => EventSubMessageType.Notification,
                "session_reconnect" => EventSubMessageType.Reconnect,
                "revocation"        => EventSubMessageType.Revocation,
                _ => throw new ArgumentException("invalid string", nameof(s)),
            };
        }
    }
}
