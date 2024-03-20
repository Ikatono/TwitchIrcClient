using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLogger.IRC.Messages
{
    public class NamReply : ReceivedMessage
    {
        public IEnumerable<string> Users =>
            Parameters.Last().Split(' ', StringSplitOptions.TrimEntries
                | StringSplitOptions.RemoveEmptyEntries);

        public NamReply(ReceivedMessage message) : base(message)
        {
            Debug.Assert(MessageType == IrcMessageType.RPL_NAMREPLY,
                $"{nameof(NamReply)} must have type {IrcMessageType.RPL_NAMREPLY}" +
                $" but has {MessageType}");
        }
    }
}
