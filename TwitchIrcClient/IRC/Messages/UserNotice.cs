using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Numerics;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TwitchIrcClient.IRC.Messages
{
    public class UserNotice : ReceivedMessage
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
        public string Channel => Parameters.FirstOrDefault("").TrimStart('#');
        public string Message => Parameters.ElementAtOrDefault(1) ?? "";
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
                throw new NotImplementedException();

            }
        }
        /// <summary>
        /// An ID that uniquely identifies the message.
        /// </summary>
        public string Id => TryGetTag("id");
        public UserNoticeType? UserNoticeType => Enum.TryParse(TryGetTag("msg-id"), out UserNoticeType type)
            ? type : null;
        public string Login => TryGetTag("login");
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
        /// 
        /// </summary>
        public string SystemMessage => TryGetTag("system-msg");
        /// <summary>
        /// When the Twitch IRC server received the message
        /// </summary>
        public DateTime Timestamp
        { get
            {
                if (double.TryParse(TryGetTag("tmi-sent-ts"), out double value))
                    return DateTime.UnixEpoch.AddMilliseconds(value);
                throw new InvalidDataException();
            }
        }
        /// <summary>
        /// Included only with <see cref="UserNoticeType.raid"/> notices.
        /// The display name of the broadcaster raiding this channel.
        /// </summary>
        public string RaidingChannelDisplayName => TryGetTag("msg-param-displayName");
        /// <summary>
        /// Included only with <see cref="UserNoticeType.raid"/> notices.
        /// The login name of the broadcaster raiding this channel.
        /// </summary>
        public string RaidingChannelLogin => TryGetTag("msg-param-login");
        /// <summary>
        /// Included only with <see cref="UserNoticeType.anongiftpaidupgrade"/> and <see cref="UserNoticeType.giftpaidupgrade"/> notices.
        /// The subscriptions promo, if any, that is ongoing (for example, Subtember 2018).
        /// </summary>
        public string SubscriptionPromoName => TryGetTag("msg-param-promo-name");
        /// <summary>
        /// Included only with <see cref="UserNoticeType.anongiftpaidupgrade"/> and
        /// <see cref="UserNoticeType.giftpaidupgrade"/> notices.
        /// The number of gifts the gifter has given during the promo indicated by <see cref="SubscriptionPromoName"/>.
        /// </summary>
        public int SubscriptionPromoCount => int.TryParse(TryGetTag("msg-param-promo-gift-total"),
            out int value) ? value : 0;
        /// <summary>
        /// Included only with <see cref="UserNoticeType.subgift"/> notices.
        /// The display name of the subscription gift recipient.
        /// </summary>
        public string RecipientDisplayName => TryGetTag("msg-param-recipient-display-name");
        /// <summary>
        /// Included only with <see cref="UserNoticeType.subgift"/> notices.
        /// The user ID of the subscription gift recipient.
        /// </summary>
        public string RecipientId => TryGetTag("msg-param-recipient-id");
        /// <summary>
        /// Included only with <see cref="UserNoticeType.subgift"/> notices.
        /// The user name of the subscription gift recipient.
        /// </summary>
        public string RecipientUsername => TryGetTag("msg-param-recipient-user-name");
        /// <summary>
        /// Only Included in <see cref="UserNoticeType.sub"/>, <see cref="UserNoticeType.resub"/>,
        /// and <see cref="UserNoticeType.subgift"/>.
        /// Either "msg-param-cumulative-months" or "msg-param-months" depending
        /// on the notice type.
        /// </summary>
        public int TotalMonths
        { get
            {
                var s1 = TryGetTag("msg-param-cumulative-months");
                var s2 = TryGetTag("msg-param-months");
                if (int.TryParse(s1, out int value1))
                    return value1;
                if (int.TryParse(s2, out int value2))
                    return value2;
                return 0;
            }
        }
        /// <summary>
        /// Included only with <see cref="UserNoticeType.sub"/> and <see cref="UserNoticeType.resub"/> notices.
        /// A Boolean value that indicates whether the user wants their streaks shared.
        /// Is "false" for other message types.
        /// </summary>
        public bool ShouldShareStreak => TryGetTag("msg-param-should-share-streak")
            == "1" ? true : false;
        /// <summary>
        /// Included only with <see cref="UserNoticeType.sub"/> and <see cref="UserNoticeType.resub"/> notices.
        /// The number of consecutive months the user has subscribed.
        /// This is zero(0) if <see cref="ShouldShareStreak"/> is 0.
        /// </summary>
        public int StreakMonths => int.TryParse(TryGetTag("msg-param-streak-months"),
            out int value) ? value : 0;
        /// <summary>
        /// Included only with <see cref="UserNoticeType.sub"/>, <see cref="UserNoticeType.resub"/>
        /// and <see cref="UserNoticeType.subgift"/> notices.
        /// </summary>
        public SubType SubPlan
        { get
            {
                switch (TryGetTag("msg-param-sub-plan").ToUpper())
                {
                    case "PRIME":
                        return SubType.Prime;
                    case "1000":
                        return SubType.T1;
                    case "2000":
                        return SubType.T2;
                    case "3000":
                        return SubType.T3;
                    default:
                        return SubType.None;
                }
            }
        }
        /// <summary>
        /// Included only with <see cref="UserNoticeType.sub"/>, <see cref="UserNoticeType.resub"/>,
        /// and <see cref="UserNoticeType.subgift"/> notices.
        /// The display name of the subscription plan. This may be a default name or one created
        /// by the channel owner.
        /// </summary>
        public string SubPlanName => TryGetTag("msg-param-sub-plan-name");
        /// <summary>
        /// Included only with <see cref="UserNoticeType.raid"/> notices.
        /// The number of viewers raiding this channel from the broadcaster’s channel.
        /// </summary>
        public int ViewerCount => int.TryParse(TryGetTag("msg-param-viewerCount"),
            out int value) ? value : 0;
        /// <summary>
        /// The type of user sending the whisper message.
        /// </summary>
        public UserType UserType
        { get
            {
                var value = TryGetTag("user-type");
                return value.ToUpper() switch
                {
                    "ADMIN"      => UserType.Admin,
                    "GLOBAL_MOD" => UserType.GlobalMod,
                    "STAFF"      => UserType.Staff,
                    ""           => UserType.Normal,
                    _            => UserType.Normal,
                };
            }
        }
        /// <summary>
        /// Included only with <see cref="UserNoticeType.ritual"/> notices.
        /// The name of the ritual being celebrated.
        /// </summary>
        public RitualType RitualType => Enum.TryParse(TryGetTag("msg-param-ritual-name"),
            out RitualType rt) ? rt : RitualType.None;
        //TODO possibly deprecate and add an int version in the future if all tiers are numeric
        /// <summary>
        /// Included only with <see cref="UserNoticeType.bitsbadgetier"/> notices.
        /// The tier of the Bits badge the user just earned. For example, 100, 1000, or 10000.
        /// </summary>
        public string Threshold => TryGetTag("msg-param-threshold");
        /// <summary>
        /// Included only with <see cref="UserNoticeType.subgift"/> notices.
        /// The number of months gifted as part of a single, multi-month gift.
        /// </summary>
        public int GiftMonths => int.TryParse(TryGetTag("msg-param-gift-months"),
            out int value) ? value : 0;
        public UserNotice(ReceivedMessage message) : base(message)
        {
            Debug.Assert(MessageType == IrcMessageType.USERNOTICE,
                $"{nameof(UserNotice)} must have type {IrcMessageType.USERNOTICE}" +
                $" but has {MessageType}");
        }
    }
    public enum UserNoticeType
    {
        sub             = 0,
        resub           = 1,
        subgift         = 2,
        submysterygift  = 3,
        giftpaidupgrade = 4,
        rewardgift      = 5,
        anongiftpaidupgrade = 6,
        raid            = 7,
        unraid          = 8,
        ritual          = 9,
        bitsbadgetier   = 10,
    }
    public enum RitualType
    {
        new_chatter = 0,

        None        = int.MinValue,
    }
    public enum SubType
    {
        Prime   = 0,
        T1      = 1,
        T2      = 2,
        T3      = 3,

        None    = int.MinValue,
    }
}
