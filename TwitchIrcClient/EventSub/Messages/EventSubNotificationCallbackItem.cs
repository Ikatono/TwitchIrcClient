using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchIrcClient.IRC.Messages;
using TwitchIrcClient.IRC;

namespace TwitchIrcClient.EventSub.Messages
{
    public delegate void EventSubMessageCallback(EventSubWebsocketClient origin, EventSubNotification message);
    public readonly record struct EventSubNotificationCallbackItem(
        EventSubMessageCallback Callback,
        IReadOnlyCollection<string>? CallbackTypes)
    {
        public bool TryCall(EventSubWebsocketClient origin, EventSubNotification message)
        {
            if (CallbackTypes?.Contains(message.Metadata.MessageType) ?? true)
            {
                Callback(origin, message);
                return true;
            }
            return false;
        }
    }
}
