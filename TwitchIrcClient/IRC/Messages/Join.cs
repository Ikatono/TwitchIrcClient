﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLogger.IRC.Messages
{
    public class Join : ReceivedMessage
    {
        public string Username => Prefix?.Split('!', 2).First() ?? "";
        public Join(ReceivedMessage message) : base(message)
        {
            Debug.Assert(MessageType == IrcMessageType.JOIN,
                $"{nameof(Join)} must have type {IrcMessageType.JOIN}" +
                $" but has {MessageType}");
        }
    }
}
