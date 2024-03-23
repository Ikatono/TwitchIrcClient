using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIrcClient.IRC.Messages
{
    public class UserState : ReceivedMessage
    {
        public string Channel => Parameters.FirstOrDefault("").TrimStart('#');
        /// <summary>
        /// The color of the user’s name in the chat room. This is a hexadecimal
        /// RGB color code in the form, #<RGB>. This tag may be empty if it is never set.
        /// </summary>
        public Color? Color
        { get
            {
                //should have format "#RRGGBB"
                if (!MessageTags.TryGetValue("color", out string? value))
                    return null;
                if (value.Length < 7)
                    return null;
                int r = Convert.ToInt32(value.Substring(1, 2), 16);
                int g = Convert.ToInt32(value.Substring(3, 2), 16);
                int b = Convert.ToInt32(value.Substring(5, 2), 16);
                return System.Drawing.Color.FromArgb(r, g, b);
            }
        }
        /// <summary>
        /// The user’s display name, escaped as described in the IRCv3 spec.
        /// </summary>
        public string DisplayName => TryGetTag("display-name");
        /// <summary>
        /// A comma-delimited list of IDs that identify the emote sets that the user has
        /// access to. Is always set to at least zero (0). To access the emotes in the set,
        /// use the Get Emote Sets API.
        /// </summary>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-emote-sets"/>
        public IEnumerable<int> EmoteSets
        { get
            {
                var value = TryGetTag("emote-sets");
                foreach (var s in value.Split(','))
                {
                    if (int.TryParse(s, out int num))
                        yield return num;
                    else
                        throw new InvalidDataException();
                }
            }
        }
        /// <summary>
        /// If a privmsg was sent, an ID that uniquely identifies the message.
        /// </summary>
        public string Id => TryGetTag("id");
        /// <summary>
        /// A Boolean value that determines whether the user is a moderator.
        /// </summary>
        public bool Moderator
        { get
            {
                if (!MessageTags.TryGetValue("mod", out string? value))
                    return false;
                return value == "1";
            }
        }
        /// <summary>
        /// Whether the user is subscribed to the channel
        /// </summary>
        public bool Subscriber
        { get
            {
                if (!MessageTags.TryGetValue("subscriber", out string? value))
                    return false;
                return value == "1";
            }
        }
        /// <summary>
        /// A Boolean value that indicates whether the user has site-wide commercial
        /// free mode enabled
        /// </summary>
        public bool Turbo
        { get
            {
                if (!MessageTags.TryGetValue("turbo", out string? value))
                    return false;
                return value == "1";
            }
        }
        public UserType UserType
        { get
            {
                if (!MessageTags.TryGetValue("user-type", out string? value))
                    return UserType.Normal;
                switch (value)
                {
                    case "admin":
                        return UserType.Admin;
                    case "global_mod":
                        return UserType.GlobalMod;
                    case "staff":
                        return UserType.Staff;
                    default:
                        return UserType.Normal;
                }
            }
        }

        public UserState(ReceivedMessage other) : base(other)
        {
            Debug.Assert(MessageType == IrcMessageType.USERSTATE,
                $"{nameof(UserState)} must have type {IrcMessageType.USERSTATE}" +
                $" but has {MessageType}");
        }
    }
}
