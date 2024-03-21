using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchIrcClient.IRC.Messages;

namespace TwitchIrcClient.IRC
{
    //public class IrcMessageEventArgs(ReceivedMessage message) : EventArgs
    //{
    //    public ReceivedMessage Message = message;
    //}
    public delegate void MessageCallback(IrcConnection origin, ReceivedMessage message);
    /// <summary>
    /// Callback to be run for received messages of specific types.
    /// </summary>
    /// <param name="Callback"></param>
    /// <param name="CallbackTypes">set to null to run for all message types</param>
    public readonly record struct MessageCallbackItem(
        MessageCallback Callback,
        IReadOnlyList<IrcMessageType>? CallbackTypes)
    {
        public bool TryCall(IrcConnection origin, ReceivedMessage message)
        {
            if (CallbackTypes?.Contains(message.MessageType) ?? true)
            {
                Callback(origin, message);
                return true;
            }
            return false;
        }
    }
    public class UserChangeEventArgs(IList<string> joined, IList<string> left) : EventArgs
    {
        public readonly IList<string> Joined = joined;
        public IList<string> Left = left;
    }
}
