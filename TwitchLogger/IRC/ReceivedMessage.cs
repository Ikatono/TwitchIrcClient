using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLogger.IRC
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
        public IrcMessageType MessageType { get; private set; }
        public string? Prefix { get; private set; }
        public string? Source { get; private set; }
        public List<string> Parameters { get; } = [];
        public string RawParameters { get; private set; }
        public string RawText { get; private set; }
        public MessageTags MessageTags { get; private set; } = [];

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
            message.MessageType = IrcReceiveMessageTypeHelper.Parse(spl_command[0]);
            //message has parameters
            if (spl_command.Length >= 2)
            {
                s = spl_command[1];
                message.RawParameters = s;
                var spl_final = s.Split(':', 2);
                var spl_initial = spl_final[0].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                message.Parameters.AddRange(spl_initial);
                if (spl_final.Length >= 2)
                    message.Parameters.Add(spl_final[1]);
            }
            return message;
        }
    }
}
