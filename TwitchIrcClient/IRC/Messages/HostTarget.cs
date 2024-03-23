using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIrcClient.IRC.Messages
{
    public class HostTarget : ReceivedMessage
    {
        /// <summary>
        /// The channel that’s hosting the viewers.
        /// </summary>
        public string HostingChannel => Parameters.FirstOrDefault("").TrimStart('#');
        public string ChannelBeingHosted =>
            Parameters.Last().Split(' ').First().TrimStart('-');
        /// <summary>
        /// true if the channel is now hosting another channel, false if it stopped hosting
        /// </summary>
        public bool NowHosting => !Parameters.Last().StartsWith('-');
        public int NumberOfViewers
        { get
            {
                var s = Parameters.LastOrDefault("");
                var s2 = s.Split(' ', StringSplitOptions.TrimEntries).LastOrDefault("");
                if (int.TryParse(s2, out int value))
                    return value;
                return 0;
            }
        }


        public HostTarget(ReceivedMessage other) : base(other)
        {
            Debug.Assert(MessageType == IrcMessageType.HOSTTARGET,
                $"{nameof(HostTarget)} must have type {IrcMessageType.HOSTTARGET}" +
                $" but has {MessageType}");
        }
    }
}
