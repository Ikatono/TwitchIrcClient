using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIrcClient.IRC.Messages
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This is designed according to <see href="https://ircv3.net/specs/extensions/message-tags.html"/>
    /// but only implementing features used by Twitch chat. See <see href="https://dev.twitch.tv/docs/irc/"/> for
    /// specifics about Twitch chat's use of IRC. Currently messages are not fully validated.
    /// </remarks>
    public class ReceivedMessage
    {
        public IrcMessageType MessageType { get; protected set; }
        public string? Prefix { get; protected set; }
        public string? Source { get; protected set; }
        public List<string> Parameters { get; } = [];
        public string RawParameters { get; protected set; }
        public string RawText { get; protected set; }
        public MessageTags MessageTags { get; protected set; } = [];

        protected ReceivedMessage()
        {

        }
        protected ReceivedMessage(ReceivedMessage other)
        {
            MessageType = other.MessageType;
            Prefix = other.Prefix;
            Source = other.Source;
            Parameters = [.. other.Parameters];
            RawParameters = other.RawParameters;
            RawText = other.RawText;
            MessageTags = [.. other.MessageTags];
        }

        /// <summary>
        /// Parses an IRC message into the proper message type
        /// </summary>
        /// <param name="s"></param>
        /// <returns>the parsed message</returns>
        public static ReceivedMessage Parse(string s)
        {
            ReceivedMessage message = new();
            message.RawText = s;
            //message has tags
            if (s.StartsWith('@'))
            {
                s = s[1..];
                //first ' ' acts as the delimeter
                var split = s.Split(' ', 2);
                Debug.Assert(split.Length == 2, "no space found to end tag section");
                string tagString = split[0];
                s = split[1].TrimStart(' ');
                message.MessageTags = MessageTags.Parse(tagString);
            }
            //message has source
            if (s.StartsWith(':'))
            {
                s = s[1..];
                var split = s.Split(' ', 2);
                Debug.Assert(split.Length == 2, "no space found to end prefix");
                message.Prefix = split[0];
                s = split[1].TrimStart(' ');
            }
            var spl_command = s.Split(' ', 2);
            message.MessageType = IrcMessageTypeHelper.Parse(spl_command[0]);
            //message has parameters
            if (spl_command.Length >= 2)
            {
                s = spl_command[1];
                message.RawParameters = s;
                //message has single parameter marked as the final parameter
                //this needs to be handled specially because the leading ' '
                //is stripped
                if (s.StartsWith(':'))
                {
                    message.Parameters.Add(s[1..]);
                }
                else
                {
                    var spl_final = s.Split(" :", 2);
                    var spl_initial = spl_final[0].Split(' ', StringSplitOptions.RemoveEmptyEntries
                        | StringSplitOptions.TrimEntries);
                    message.Parameters.AddRange(spl_initial);
                    if (spl_final.Length >= 2)
                        message.Parameters.Add(spl_final[1]);
                }
            }
            return message.MessageType switch
            {
                IrcMessageType.CLEARCHAT => new ClearChat(message),
                IrcMessageType.CLEARMSG => new ClearMsg(message),
                IrcMessageType.JOIN => new Join(message),
                IrcMessageType.GLOBALUSERSTATE => new GlobalUserState(message),
                IrcMessageType.HOSTTARGET => new HostTarget(message),
                IrcMessageType.NOTICE => new Notice(message),
                IrcMessageType.PART => new Part(message),
                IrcMessageType.PRIVMSG => new Privmsg(message),
                IrcMessageType.ROOMSTATE => new Roomstate(message),
                IrcMessageType.RPL_NAMREPLY => new NamReply(message),
                IrcMessageType.USERNOTICE => new UserNotice(message),
                IrcMessageType.USERSTATE => new UserState(message),
                IrcMessageType.WHISPER => new Whisper(message),
                _ => message,
            };
        }
        /// <summary>
        /// Tries to get the value of the tag.
        /// </summary>
        /// <param name="s"></param>
        /// <returns>the value of the tag, or "" if not found</returns>
        protected string TryGetTag(string s)
        {
            if (!MessageTags.TryGetValue(s, out string? value))
                return "";
            return value ?? "";
        }
    }
}
