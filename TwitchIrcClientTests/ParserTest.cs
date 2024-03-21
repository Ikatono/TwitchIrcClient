using System.Drawing;
using System;
using TwitchIrcClient.IRC;
using TwitchIrcClient.IRC.Messages;

namespace TwitchIrcClientTests
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void TestSimpleMessages()
        {
            var ROOMSTATE = "@emote-only=0;followers-only=-1;r9k=0;room-id=321654987;slow=0;subs-only=0 :tmi.twitch.tv ROOMSTATE #channelname";
            var NAMREPLY = ":justinfan7550.tmi.twitch.tv 353 justinfan7550 = #channelname :user1 user2 user3 user4 user5";
            var JOIN = ":newuser!newuser@newuser.tmi.twitch.tv JOIN #channelname";
            var PART = ":leavinguser!leavinguser@leavinguser.tmi.twitch.tv PART #channelname";
            var PRIVMSG = "@badge-info=subscriber/1;badges=subscriber/0;client-nonce=202e32113a3768963eded865e051fc5b;color=#AAAAFF;" +
                "display-name=ChattingUser;emotes=;first-msg=0;flags=;id=24fe75a1-06a5-4078-a31f-cf615107b2a2;mod=0;returning-chatter=0;" +
                "room-id=321654987;subscriber=1;tmi-sent-ts=1710920497332;turbo=0;user-id=01234567;user-type= " +
                ":chattinguser!chattinguser@chattinguser.tmi.twitch.tv PRIVMSG #channelname :This is a test chat message";
            var CHEER = "@badge-info=subscriber/9;badges=subscriber/9,twitch-recap-2023/1;bits=100;color=#FF0000;display-name=CheeringUser;" +
                //I haven't fixed this emote tag after rewriting the message
                "emotes=emotesv2_44a39d65e08f43adac871a80e9b96d85:17-24;first-msg=1;flags=;id=5eab1319-5d46-4c55-be29-33c2f834e42e;mod=0;" +
                "returning-chatter=0;room-id=321654987;subscriber=0;tmi-sent-ts=1710920826069;turbo=1;user-id=012345678;user-type=;vip " +
                ":cheeringuser!cheeringuser@cheeringuser.tmi.twitch.tv PRIVMSG #channelname :This includes a cheer Cheer100";
            //var CLEARMSG = "";
            //var CLEARROOM = "";

            var _roomstate = ReceivedMessage.Parse(ROOMSTATE);
            Assert.AreEqual(IrcMessageType.ROOMSTATE, _roomstate.MessageType);
            if (_roomstate is Roomstate roomstate)
            {
                Assert.AreEqual("channelname", roomstate.ChannelName);
                Assert.IsTrue(roomstate.MessageTags.TryGetValue("emote-only", out string emoteOnly));
                Assert.AreEqual("0", emoteOnly);
                Assert.IsFalse(roomstate.EmoteOnly);
                Assert.IsTrue(roomstate.MessageTags.TryGetValue("followers-only", out string followersOnly));
                Assert.AreEqual("-1", followersOnly);
                Assert.AreEqual(-1, roomstate.FollowersOnly);
                Assert.IsTrue(roomstate.MessageTags.TryGetValue("r9k", out string r9k));
                Assert.AreEqual("0", r9k);
                Assert.IsFalse(roomstate.UniqueMode);
                Assert.IsTrue(roomstate.MessageTags.TryGetValue("room-id", out string roomId));
                Assert.AreEqual("321654987", roomId);
                Assert.AreEqual("321654987", roomstate.RoomId);
                Assert.IsTrue(roomstate.MessageTags.TryGetValue("slow", out string slow));
                Assert.AreEqual("0", slow);
                Assert.AreEqual(0, roomstate.Slow);
                Assert.IsTrue(roomstate.MessageTags.TryGetValue("subs-only", out string subsOnly));
                Assert.AreEqual("0", subsOnly);
                Assert.AreEqual(false, roomstate.SubsOnly);
            }
            else
            {
                Assert.Fail();
            }

            var _namReply = ReceivedMessage.Parse(NAMREPLY);
            Assert.AreEqual(IrcMessageType.RPL_NAMREPLY, _namReply.MessageType);
            if (_namReply is NamReply namReply)
            {
                Assert.AreEqual("channelname", namReply.ChannelName);
                Assert.IsTrue("user1 user2 user3 user4 user5".Split().Order()
                    .SequenceEqual(namReply.Users.Order()));
            }
            else
            {
                Assert.Fail();
            }

            var _join = ReceivedMessage.Parse(JOIN);
            Assert.AreEqual(IrcMessageType.JOIN, _join.MessageType);
            if (_join is Join join)
            {
                Assert.AreEqual("channelname", join.ChannelName);
                Assert.AreEqual("newuser", join.Username);
            }
            else
            {
                Assert.Fail();
            }

            var _part = ReceivedMessage.Parse(PART);
            Assert.AreEqual(IrcMessageType.PART, _part.MessageType);
            if (_part is Part part)
            {
                Assert.AreEqual("channelname", part.ChannelName);
                Assert.AreEqual("leavinguser", part.Username);
            }
            else
            {
                Assert.Fail();
            }

            var _priv = ReceivedMessage.Parse(PRIVMSG);
            Assert.AreEqual(IrcMessageType.PRIVMSG, _priv.MessageType);
            if (_priv is Privmsg priv)
            {
                Assert.AreEqual("This is a test chat message", priv.ChatMessage);
                Assert.AreEqual(0, priv.Bits);
                Assert.AreEqual("ChattingUser", priv.DisplayName);
                Assert.AreEqual(Color.FromArgb(170, 170, 255), priv.Color);
                Assert.AreEqual("24fe75a1-06a5-4078-a31f-cf615107b2a2", priv.Id);
                Assert.IsFalse(priv.FirstMessage);
                Assert.IsFalse(priv.Moderator);
                Assert.AreEqual("321654987", priv.RoomId);
                Assert.IsTrue(priv.Subscriber);
                Assert.IsFalse(priv.Turbo);
                Assert.AreEqual("01234567", priv.UserId);
                Assert.AreEqual(UserType.Normal, priv.UserType);
                Assert.IsFalse(priv.Vip);
                Assert.AreEqual(new DateTime(2024, 3, 20, 7, 41, 37, 332, DateTimeKind.Utc), priv.Timestamp);
            }
            else
            {
                Assert.Fail();
            }

            var _cheer = ReceivedMessage.Parse(CHEER);
            Assert.AreEqual(IrcMessageType.PRIVMSG, _cheer.MessageType);
            if (_cheer is Privmsg cheer)
            {
                Assert.AreEqual("This includes a cheer Cheer100", cheer.ChatMessage);
                Assert.AreEqual(100, cheer.Bits);
                Assert.AreEqual("CheeringUser", cheer.DisplayName);
                Assert.AreEqual(Color.FromArgb(255, 0, 0), cheer.Color);
                Assert.AreEqual("5eab1319-5d46-4c55-be29-33c2f834e42e", cheer.Id);
                Assert.IsTrue(cheer.FirstMessage);
                Assert.IsFalse(cheer.Moderator);
                Assert.AreEqual("321654987", cheer.RoomId);
                Assert.IsFalse(cheer.Subscriber);
                Assert.IsTrue(cheer.Turbo);
                Assert.AreEqual("012345678", cheer.UserId);
                Assert.AreEqual(UserType.Normal, cheer.UserType);
                Assert.IsTrue(cheer.Vip);
                //test that timestamp is within 1 second
                Assert.AreEqual(new DateTime(2024, 3, 20, 7, 47, 6, 069, DateTimeKind.Utc), cheer.Timestamp);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}