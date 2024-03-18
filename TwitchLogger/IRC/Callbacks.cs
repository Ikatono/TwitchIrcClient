using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLogger.IRC
{
    public class IrcMessageEventArgs(ReceivedMessage message) : EventArgs
    {
        public ReceivedMessage Message = message;
    }
    public delegate void IrcCallback(ReceivedMessage message);
    /// <summary>
    /// Callback to be run for received messages of specific types.
    /// </summary>
    /// <param name="Callback"></param>
    /// <param name="CallbackTypes">set to null to run for all message types</param>
    public readonly record struct CallbackItem(
        IrcCallback Callback,
        IReadOnlyList<IrcMessageType>? CallbackTypes)
    {
        public bool TryCall(ReceivedMessage message)
        {
            if (CallbackTypes?.Contains(message.MessageType) ?? true)
            {
                Callback(message);
                return true;
            }
            return false;
        }
    }
}
