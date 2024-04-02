using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchIrcClient.PubSub.Message;

namespace TwitchIrcClient.PubSub
{
    public delegate void PubSubCallback(PubSubMessage message, PubSubConnection connection);
    public record struct PubSubCallbackItem(PubSubCallback Callback, IList<string>? Types)
    {
        public readonly bool MaybeRunCallback(PubSubMessage message, PubSubConnection connection)
        {
            if (Types is null || Types.Contains(message.TypeString))
            {
                Callback?.Invoke(message, connection);
                return true;
            }
            return false;
        }
    }
}
