using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TwitchIrcClient.IRC;
using TwitchIrcClient.IRC.Messages;

namespace TwitchIrcClient.IRC.Messages
{
    public class Roomstate : ReceivedMessage
    {
        /// <summary>
        /// A Boolean value that determines whether the chat room allows only messages with emotes.
        /// </summary>
        public bool EmoteOnly
        { get
            {
                var value = TryGetTag("emote-only");
                if (value == "1")
                    return true;
                if (value == "0")
                    return false;
                throw new InvalidDataException($"tag \"emote-only\" does not have a proper value: {value}");
            }
        }
        /// <summary>
        /// An integer value that determines whether only followers can post messages in the chat room.
        /// The value indicates how long, in minutes, the user must have followed the broadcaster before
        /// posting chat messages. If the value is -1, the chat room is not restricted to followers only.
        /// </summary>
        public int FollowersOnly
        { get
            {
                var value = TryGetTag("followers-only");
                if (!int.TryParse(value, out int result))
                    throw new InvalidDataException();
                return result;
            }
        }
        /// <summary>
        /// A Boolean value that determines whether a user’s messages must be unique.
        /// Applies only to messages with more than 9 characters.
        /// </summary>
        public bool UniqueMode
        { get
            {
                var value = TryGetTag("r9k");
                if (value == "1")
                    return true;
                if (value == "0")
                    return false;
                throw new InvalidDataException($"tag \"r9k\" does not have a proper value: {value}");
            }
        }
        /// <summary>
        /// An ID that identifies the chat room (channel).
        /// </summary>
        public string RoomId => TryGetTag("room-id");
        /// <summary>
        /// An integer value that determines how long, in seconds, users must wait between sending messages.
        /// </summary>
        public int Slow
        { get
            {
                string value = TryGetTag("slow");
                if (!int.TryParse(value, out int result))
                    throw new InvalidDataException($"tag \"slow\" does not have a proper value: {value}");
                return result;
            }
        }
        /// <summary>
        /// A Boolean value that determines whether only subscribers and moderators can chat in the chat room.
        /// </summary>
        public bool SubsOnly
        { get
            {
                var value = TryGetTag("subs-only");
                if (value == "1")
                    return true;
                if (value == "0")
                    return false;
                throw new InvalidDataException($"tag \"subs-only\" does not have a proper value: {value}");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string ChannelName => Parameters.Last().TrimStart('#');
        public Roomstate(ReceivedMessage other) : base(other)
        {
            Debug.Assert(MessageType == IrcMessageType.ROOMSTATE,
                $"{nameof(Roomstate)} must have type {IrcMessageType.ROOMSTATE}" +
                $" but has {MessageType}");
        }
    }
}
