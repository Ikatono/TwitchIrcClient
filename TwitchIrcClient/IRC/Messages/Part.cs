using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIrcClient.IRC.Messages
{
    public class Part : ReceivedMessage
    {
        public string Username => Prefix?.Split('!', 2).First() ?? "";
        public string ChannelName => Parameters.Single().TrimStart('#');
        public Part(ReceivedMessage message) : base(message)
        {
            Debug.Assert(MessageType == IrcMessageType.PART,
                $"{nameof(Part)} must have type {IrcMessageType.PART}" +
                $" but has {MessageType}");
        }
    }
}
