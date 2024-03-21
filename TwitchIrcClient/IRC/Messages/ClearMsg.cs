using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIrcClient.IRC.Messages
{
    /// <summary>
    /// Indicates that a message was deleted.
    /// </summary>
    public class ClearMsg : ReceivedMessage
    {
        /// <summary>
        /// The user who sent the deleted message.
        /// </summary>
        public string Login => TryGetTag("login");
        /// <summary>
        /// Optional. The ID of the channel (chat room) where the
        /// message was removed from.
        /// </summary>
        public string RoomId => TryGetTag("room-id");
        /// <summary>
        /// A UUID that identifies the message that was removed.
        /// </summary>
        public string TargetMessageId => TryGetTag("target-msg-id");
        /// <summary>
        /// 
        /// </summary>
        public DateTime? TmiSentTime
        { get
            {
                string s = TryGetTag("tmi-sent-ts");
                if (!double.TryParse(s, out double d))
                    return null;
                return DateTime.UnixEpoch.AddSeconds(d / 1000);
            } 
        }
        public ClearMsg(ReceivedMessage message) : base(message)
        {
            Debug.Assert(MessageType == IrcMessageType.CLEARMSG,
                $"{nameof(ClearMsg)} must have type {IrcMessageType.CLEARMSG}" +
                $" but has {MessageType}");
        }
    }
}
