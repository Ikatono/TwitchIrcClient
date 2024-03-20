using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLogger.IRC.Messages
{
    public class UserNotice : ReceivedMessage
    {
        /// <summary>
        /// List of chat badges. Most badges have only 1 version, but some badges like
        /// subscriber badges offer different versions of the badge depending on how
        /// long the user has subscribed. To get the badge, use the Get Global Chat
        /// Badges and Get Channel Chat Badges APIs.Match the badge to the set-id field’s
        /// value in the response.Then, match the version to the id field in the list of versions.
        /// </summary>
        public List<Badge> Badges
        { get
            {
                string value = TryGetTag("badges");
                if (value == "")
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
        /// <summary>
        /// An ID that uniquely identifies the message.
        /// </summary>
        public string Id => TryGetTag("id");
        public UserNoticeType? UserNoticeType => Enum.TryParse(TryGetTag("msg-id"), out UserNoticeType type)
            ? type : null;
        /// <summary>
        /// Whether the user is a moderator in this channel
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
        /// An ID that identifies the chat room (channel).
        /// </summary>
        public string RoomId => TryGetTag("room-id");
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
        /// <summary>
        /// The user’s ID
        /// </summary>
        public string UserId
        { get
            {
                if (!MessageTags.TryGetValue("user-id", out string? value))
                    return "";
                return value ?? "";
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
        public UserNotice(ReceivedMessage message) : base(message)
        {
            Debug.Assert(MessageType == IrcMessageType.USERNOTICE,
                $"{nameof(UserNotice)} must have type {IrcMessageType.USERNOTICE}" +
                $" but has {MessageType}");
        }
    }
    public enum UserNoticeType
    {
        sub,
        resub,
        subgift,
        submysterygift,
        giftpaidupgrade,
        rewardgift,
        anongiftpaidupgrade,
        raid,
        unraid,
        ritual,
        bitsbadgetier,
    }
}
