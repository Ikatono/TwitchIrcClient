using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIrcClient.IRC.Messages
{
    public class NamReply : ReceivedMessage
    {
        public IEnumerable<string> Users =>
            Parameters.Last().Split(' ', StringSplitOptions.TrimEntries
                | StringSplitOptions.RemoveEmptyEntries);
        public string ChannelName => Parameters.TakeLast(2).First().TrimStart('#');
        public NamReply(ReceivedMessage message) : base(message)
        {
            Debug.Assert(MessageType == IrcMessageType.RPL_NAMREPLY,
                $"{nameof(NamReply)} must have type {IrcMessageType.RPL_NAMREPLY}" +
                $" but has {MessageType}");
        }
    }
}
