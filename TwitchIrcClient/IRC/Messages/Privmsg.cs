﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TwitchLogger.IRC.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public class Privmsg : ReceivedMessage
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
        /// The amount of bits cheered. Equals 0 if message did not contain a cheer.
        /// </summary>
        public int Bits
        { get
            {
                if (!MessageTags.TryGetValue("bits", out string? value))
                    return 0;
                if (!int.TryParse(value, out int bits))
                    return 0;
                return bits;
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
        /// <summary>
        /// A Boolean value that determines whether the user that sent the chat is a VIP.
        /// </summary>
        public bool Vip => MessageTags.ContainsKey("vip");
        public string ChatMessage => Parameters.Last();
        public Privmsg(ReceivedMessage message) : base(message)
        {
            Debug.Assert(MessageType == IrcMessageType.PRIVMSG,
                $"{nameof(Privmsg)} must have type {IrcMessageType.PRIVMSG}" +
                $" but has {MessageType}");
        }
    }
}