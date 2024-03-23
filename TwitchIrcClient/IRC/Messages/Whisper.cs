using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIrcClient.IRC.Messages
{
    public class Whisper : ReceivedMessage
    {

        /// <summary>
        /// List of chat badges. Most badges have only 1 version, but some badges like
        /// subscriber badges offer different versions of the badge depending on how
        /// long the user has subscribed. To get the badge, use the Get Global Chat
        /// Badges and Get Channel Chat Badges APIs. Match the badge to the set-id field’s
        /// value in the response.Then, match the version to the id field in the list of versions.
        /// </summary>
        public List<Badge> Badges
        { get
            {
                if (!MessageTags.TryGetValue("badges", out string? value))
                    return [];
                if (value == null)
                    return [];
                List<Badge> badges = [];
                foreach (var item in value.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var spl = item.Split('/', 2);
                    badges.Add(new Badge(spl[0], spl[1]));
                }
                return badges;
            }
        }
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
        /// The user’s display name. This tag may be empty if it is never set.
        /// </summary>
        public string DisplayName
        { get
            {
                if (!MessageTags.TryGetValue("display-name", out string? value))
                    return "";
                return value ?? "";
            }
        }
        public IEnumerable<Emote> Emotes
        { get
            {
                var tag = TryGetTag("emotes");
                foreach (var emote in tag.Split('/', StringSplitOptions.RemoveEmptyEntries))
                {
                    var split = emote.Split(':', 2);
                    Debug.Assert(split.Length == 2);
                    var name = split[0];
                    foreach (var indeces in split[1].Split(','))
                    {
                        var split2 = indeces.Split('-');
                        if (!int.TryParse(split2[0], out int start) ||
                            !int.TryParse(split2[1], out int end))
                            throw new InvalidDataException();
                        yield return new Emote(name, start, end - start + 1);
                    }
                }
            }
        }
        /// <summary>
        /// An ID that uniquely identifies the whisper message.
        /// </summary>
        public string MessageId => TryGetTag("message-id");
        /// <summary>
        /// An ID that uniquely identifies the whisper thread.
        /// The ID is in the form, <smaller-value-user-id>_<larger-value-user-id>.
        /// </summary>
        public string ThreadId => TryGetTag("thread-id");
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
        /// <summary>
        /// The ID of the user sending the whisper message.
        /// </summary>
        public string UserId => TryGetTag("user-id");
        public string Message => Parameters.LastOrDefault("");
        /// <summary>
        /// The type of the user. Assumes a normal user if this is not provided or is invalid.
        /// </summary>
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
        public Whisper(ReceivedMessage other) : base(other)
        {
            Debug.Assert(MessageType == IrcMessageType.WHISPER,
                $"{nameof(Whisper)} must have type {IrcMessageType.WHISPER}" +
                $" but has {MessageType}");
        }
    }
}
