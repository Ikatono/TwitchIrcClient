using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIrcClient.IRC.Messages
{
    public class ClearChat : ReceivedMessage
    {
        /// <summary>
        /// The number of seconds the user was timed out for. Is 0 if the
        /// user was permabanned or the message is a channel clear.
        /// </summary>
        public int TimeoutDuration
        { get
            {
                string s = TryGetTag("ban-duration");
                if (!int.TryParse(s, out int value))
                    return 0;
                return value;
            }
        }
        /// <summary>
        /// The ID of the channel where the messages were removed from.
        /// </summary>
        public string RoomId => TryGetTag("room-id");
        /// <summary>
        /// The ID of the user that was banned or put in a timeout.
        /// </summary>
        public string TargetUserId => TryGetTag("target-user-id");
        public DateTime? TmiSentTime
        { get
            {
                string s = TryGetTag("tmi-sent-ts");
                if (!double.TryParse(s, out double d))
                    return null;
                return DateTime.UnixEpoch.AddSeconds(d);
            }
        }
        /// <summary>
        /// true if the message permabans a user.
        /// </summary>
        public bool IsBan
        { get
            {
                return MessageTags.ContainsKey("target-user-id")
                    && !MessageTags.ContainsKey("ban-duration");
            }
        }
        /// <summary>
        /// The name of the channel that either was cleared or banned the user
        /// </summary>
        public string Channel => Parameters.First();
        /// <summary>
        /// The username of the banned user, or "" if message is a
        /// channel clear.
        /// </summary>
        public string User => Parameters.ElementAtOrDefault(2) ?? "";
        public ClearChat(ReceivedMessage message) : base(message)
        {
            Debug.Assert(MessageType == IrcMessageType.CLEARCHAT,
                $"{nameof(ClearMsg)} must have type {IrcMessageType.CLEARCHAT}" +
                $" but has {MessageType}");
        }
    }
}
