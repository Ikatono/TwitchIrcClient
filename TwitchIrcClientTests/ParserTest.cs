using System.Drawing;
using System;
using TwitchIrcClient.IRC;
using TwitchIrcClient.IRC.Messages;
using System.Diagnostics;

namespace TwitchIrcClientTests
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void TestRoomstate()
        {
            var ROOMSTATE = "@emote-only=0;followers-only=-1;r9k=0;room-id=321654987;slow=0;subs-only=0 :tmi.twitch.tv ROOMSTATE #channelname";
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
        }
        [TestMethod]
        public void TestNamreply()
        {
            var NAMREPLY = ":justinfan7550.tmi.twitch.tv 353 justinfan7550 = #channelname :user1 user2 user3 user4 user5";
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
        }
        [TestMethod]
        public void TestJoin()
        {
            var JOIN = ":newuser!newuser@newuser.tmi.twitch.tv JOIN #channelname";
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
        }
        [TestMethod]
        public void TestPart()
        {
            var PART = ":leavinguser!leavinguser@leavinguser.tmi.twitch.tv PART #channelname";
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
        }
        [TestMethod]
        public void TestPrivmsg()
        {
            var PRIVMSG = "@badge-info=subscriber/1;badges=subscriber/0;client-nonce=202e32113a3768963eded865e051fc5b;color=#AAAAFF;" +
                "display-name=ChattingUser;emotes=;first-msg=0;flags=;id=24fe75a1-06a5-4078-a31f-cf615107b2a2;mod=0;returning-chatter=0;" +
                "room-id=321654987;subscriber=1;tmi-sent-ts=1710920497332;turbo=0;user-id=01234567;user-type= " +
                ":chattinguser!chattinguser@chattinguser.tmi.twitch.tv PRIVMSG #channelname :This is a test chat message";
            var CHEER = "@badge-info=subscriber/9;badges=subscriber/9,twitch-recap-2023/1;bits=100;color=#FF0000;display-name=CheeringUser;" +
                //I haven't fixed this emote tag after rewriting the message
                "emotes=emotesv2_44a39d65e08f43adac871a80e9b96d85:17-24;first-msg=1;flags=;id=5eab1319-5d46-4c55-be29-33c2f834e42e;mod=0;" +
                "returning-chatter=0;room-id=321654987;subscriber=0;tmi-sent-ts=1710920826069;turbo=1;user-id=012345678;user-type=;vip " +
                ":cheeringuser!cheeringuser@cheeringuser.tmi.twitch.tv PRIVMSG #channelname :This includes a cheer Cheer100";
            var ESCAPE = @"@escaped=\:\s\\\r\n\a\b\c PRIVMSG #channelname :message";
            var EMOTES = @"@badge-info=subscriber/4;badges=subscriber/3;client-nonce=2cc8bb73f5d946b22ec2905c8ccdee7a;color=#1E90FF;" +
                @"display-name=Ikatono;emote-only=1;emotes=emotesv2_4f3ee26e385b46aa88d5f45307489939:0-12,14-26/emotesv2_9046ad54f76f42389edb4cc828b1b057" +
                @":28-35,37-44;first-msg=0;flags=;id=08424675-217f-44bc-b9c0-24e2e2dd5f33;mod=0;returning-chatter=0;room-id=230151386;" +
                @"subscriber=1;tmi-sent-ts=1711136008625;turbo=0;user-id=24866530;user-type= :ikatono!ikatono@ikatono.tmi.twitch.tv " +
                @"PRIVMSG #bajiru_en :bajiBUFFERING bajiBUFFERING bajiBONK bajiBONK";

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
                Assert.IsTrue(priv.Badges.SequenceEqual([new Badge("subscriber", "0")]));
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
                Assert.AreEqual(new DateTime(2024, 3, 20, 7, 47, 6, 069, DateTimeKind.Utc), cheer.Timestamp);
                Assert.IsTrue(cheer.Badges.SequenceEqual([
                    new Badge("subscriber", "9"),
                    new Badge("twitch-recap-2023", "1"),
                    ]));
            }
            else
            {
                Assert.Fail();
            }

            var _escape = ReceivedMessage.Parse(ESCAPE);
            Assert.AreEqual(IrcMessageType.PRIVMSG, _escape.MessageType);
            if (_escape is Privmsg escape)
            {
                Assert.AreEqual("; \\\r\nabc", escape.MessageTags["escaped"]);
            }
            else
            {
                Assert.Fail();
            }

            var _emotes = ReceivedMessage.Parse(EMOTES);
            Assert.AreEqual(IrcMessageType.PRIVMSG, _emotes.MessageType);
            if (_emotes is Privmsg emotes)
            {
                Assert.IsTrue(emotes.Emotes.SequenceEqual([
                    new Emote("emotesv2_4f3ee26e385b46aa88d5f45307489939", 0, 12-0+1),
                    new Emote("emotesv2_4f3ee26e385b46aa88d5f45307489939", 14, 26-14+1),
                    new Emote("emotesv2_9046ad54f76f42389edb4cc828b1b057", 28, 35-28+1),
                    new Emote("emotesv2_9046ad54f76f42389edb4cc828b1b057", 37, 44-37+1),
                    ]));
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void TestUserNotice()
        {
            //these 4 are examples given from Twitch's USERNOTICE tags page
            var RESUB = @"@badge-info=;badges=staff/1,broadcaster/1,turbo/1;color=#008000;display-name=ronni;emotes=;" +
                @"id=db25007f-7a18-43eb-9379-80131e44d633;login=ronni;mod=0;msg-id=resub;msg-param-cumulative-months=6;msg-param-streak-months=2;" +
                @"msg-param-should-share-streak=1;msg-param-sub-plan=Prime;msg-param-sub-plan-name=Prime;room-id=12345678;subscriber=1;" +
                @"system-msg=ronni\shas\ssubscribed\sfor\s6\smonths!;tmi-sent-ts=1507246572675;turbo=1;user-id=87654321;user-type=staff" +
                @" :tmi.twitch.tv USERNOTICE #dallas :Great stream -- keep it up!";
            var GIFTED = @"@badge-info=;badges=staff/1,premium/1;color=#0000FF;display-name=TWW2;emotes=;" +
                @"id=e9176cd8-5e22-4684-ad40-ce53c2561c5e;login=tww2;mod=0;msg-id=subgift;msg-param-months=1;" +
                @"msg-param-recipient-display-name=Mr_Woodchuck;msg-param-recipient-id=55554444;msg-param-recipient-name=mr_woodchuck;" +
                @"msg-param-sub-plan-name=House\sof\sNyoro~n;msg-param-sub-plan=1000;room-id=19571752;subscriber=0;" +
                @"system-msg=TWW2\sgifted\sa\sTier\s1\ssub\sto\sMr_Woodchuck!;tmi-sent-ts=1521159445153;turbo=0;user-id=87654321;user-type=staff" +
                @" :tmi.twitch.tv USERNOTICE #forstycup";
            var RAID = @"@badge-info=;badges=turbo/1;color=#9ACD32;display-name=TestChannel;emotes=;id=3d830f12-795c-447d-af3c-ea05e40fbddb;" +
                @"login=testchannel;mod=0;msg-id=raid;msg-param-displayName=TestChannel;msg-param-login=testchannel;msg-param-viewerCount=15;" +
                @"room-id=33332222;subscriber=0;system-msg=15\sraiders\sfrom\sTestChannel\shave\sjoined\n!;tmi-sent-ts=1507246572675;turbo=1;" +
                @"user-id=123456;user-type= :tmi.twitch.tv USERNOTICE #othertestchannel";
            var NEWCHATTER = @"@badge-info=;badges=;color=;display-name=SevenTest1;emotes=30259:0-6;id=37feed0f-b9c7-4c3a-b475-21c6c6d21c3d;" +
                @"login=seventest1;mod=0;msg-id=ritual;msg-param-ritual-name=new_chatter;room-id=87654321;subscriber=0;" +
                @"system-msg=Seventoes\sis\snew\shere!;tmi-sent-ts=1508363903826;turbo=0;user-id=77776666;user-type=" +
                @" :tmi.twitch.tv USERNOTICE #seventoes :HeyGuys";

            var _resub = ReceivedMessage.Parse(RESUB);
            Assert.AreEqual(IrcMessageType.USERNOTICE, _resub.MessageType);
            if (_resub is UserNotice resub)
            {
                Assert.AreEqual(Color.FromArgb(0, 128, 0), resub.Color);
                Assert.AreEqual("ronni", resub.DisplayName);
                Assert.AreEqual("db25007f-7a18-43eb-9379-80131e44d633", resub.Id);
                Assert.AreEqual("ronni", resub.Login);
                Assert.IsFalse(resub.Moderator);
                Assert.AreEqual(RitualType.None, resub.RitualType);
                Assert.AreEqual(UserNoticeType.resub, resub.UserNoticeType);
                Assert.AreEqual(6, resub.TotalMonths);
                Assert.AreEqual(2, resub.StreakMonths);
                Assert.IsTrue(resub.ShouldShareStreak);
                Assert.AreEqual(SubType.Prime, resub.SubPlan);
                Assert.AreEqual("Prime", resub.SubPlanName);
                Assert.AreEqual("12345678", resub.RoomId);
                Assert.IsTrue(resub.Subscriber);
                Assert.AreEqual("ronni has subscribed for 6 months!", resub.SystemMessage);
                Assert.AreEqual(new DateTime(2017, 10, 5, 23, 36, 12, 675, DateTimeKind.Utc),
                    resub.Timestamp);
                Assert.IsTrue(resub.Turbo);
                Assert.AreEqual("87654321", resub.UserId);
                Assert.AreEqual(UserType.Staff, resub.UserType);
                Assert.AreEqual("dallas", resub.Channel);
                Assert.AreEqual("Great stream -- keep it up!", resub.Message);
            }
            else
            {
                Assert.Fail();
            }

            var _gifted = ReceivedMessage.Parse(GIFTED);
            Assert.AreEqual(IrcMessageType.USERNOTICE, _gifted.MessageType);
            if (_gifted is UserNotice gifted)
            {
                Assert.AreEqual(Color.FromArgb(0, 0, 255), gifted.Color);
                Assert.AreEqual("TWW2", gifted.DisplayName);
                Assert.AreEqual("e9176cd8-5e22-4684-ad40-ce53c2561c5e", gifted.Id);
                Assert.AreEqual("tww2", gifted.Login);
                Assert.IsFalse(gifted.Moderator);
                Assert.AreEqual(RitualType.None, gifted.RitualType);
                Assert.AreEqual(UserNoticeType.subgift, gifted.UserNoticeType);
                Assert.AreEqual(1, gifted.TotalMonths);
                Assert.AreEqual("Mr_Woodchuck", gifted.RecipientDisplayName);
                //Twitch's example uses "msg-param-recipient-name" which doesn't appear anywhere
                //else in the documentation. I believe this was inteded to be "msg-param-recipient-user-name"
                //Assert.AreEqual("mr_woodchuck", gifted.RecipientUsername);
                Assert.AreEqual("55554444", gifted.RecipientId);
                Assert.AreEqual("House of Nyoro~n", gifted.SubPlanName);
                Assert.AreEqual(SubType.T1, gifted.SubPlan);
                Assert.AreEqual("19571752", gifted.RoomId);
                Assert.IsFalse(gifted.Subscriber);
                Assert.AreEqual("TWW2 gifted a Tier 1 sub to Mr_Woodchuck!", gifted.SystemMessage);
                Assert.AreEqual(new DateTime(2018, 3, 16, 0, 17, 25, 153, DateTimeKind.Utc),
                    gifted.Timestamp);
                Assert.IsFalse(gifted.Turbo);
                Assert.AreEqual("87654321", gifted.UserId);
                Assert.AreEqual(UserType.Staff, gifted.UserType);
                Assert.AreEqual("forstycup", gifted.Channel);
            }
            else
            {
                Assert.Fail();
            }

            var _raid = ReceivedMessage.Parse(RAID);
            Assert.AreEqual(IrcMessageType.USERNOTICE, _raid.MessageType);
            if (_raid is UserNotice raid)
            {
                Assert.AreEqual(Color.FromArgb(154, 205, 50), raid.Color);
                Assert.AreEqual("TestChannel", raid.DisplayName);
                Assert.AreEqual("3d830f12-795c-447d-af3c-ea05e40fbddb", raid.Id);
                Assert.AreEqual("testchannel", raid.Login);
                Assert.IsFalse(raid.Moderator);
                Assert.AreEqual(RitualType.None, raid.RitualType);
                Assert.AreEqual(UserNoticeType.raid, raid.UserNoticeType);
                Assert.AreEqual("TestChannel", raid.RaidingChannelDisplayName);
                Assert.AreEqual("testchannel", raid.RaidingChannelLogin);
                Assert.AreEqual(15, raid.ViewerCount);
                Assert.AreEqual("33332222", raid.RoomId);
                Assert.IsFalse(raid.Subscriber);
                Assert.AreEqual("15 raiders from TestChannel have joined\n!", raid.SystemMessage);
                Assert.AreEqual(new DateTime(2017, 10, 5, 23, 36, 12, 675, DateTimeKind.Utc),
                    raid.Timestamp);
                Assert.IsTrue(raid.Turbo);
                Assert.AreEqual("123456", raid.UserId);
                Assert.AreEqual(UserType.Normal, raid.UserType);
            }
            else
            {
                Assert.Fail();
            }

            var _newchatter = ReceivedMessage.Parse(NEWCHATTER);
            Assert.AreEqual(IrcMessageType.USERNOTICE, _newchatter.MessageType);
            if (_newchatter is UserNotice newchatter)
            {
                Assert.AreEqual(null, newchatter.Color);
                Assert.AreEqual("SevenTest1", newchatter.DisplayName);
                Assert.AreEqual("37feed0f-b9c7-4c3a-b475-21c6c6d21c3d", newchatter.Id);
                Assert.AreEqual("seventest1", newchatter.Login);
                Assert.IsFalse(newchatter.Moderator);
                Assert.AreEqual(RitualType.new_chatter, newchatter.RitualType);
                Assert.AreEqual("87654321", newchatter.RoomId);
                Assert.IsFalse(newchatter.Subscriber);
                Assert.AreEqual("Seventoes is new here!", newchatter.SystemMessage);
                Assert.AreEqual(new DateTime(2017, 10, 18, 21, 58, 23, 826, DateTimeKind.Utc),
                    newchatter.Timestamp);
                Assert.IsFalse(newchatter.Turbo);
                Assert.AreEqual("77776666", newchatter.UserId);
                Assert.AreEqual(UserType.Normal, newchatter.UserType);
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void TestUserstate()
        {
            var USERSTATE = @"@badge-info=;badges=staff/1;color=#0D4200;display-name=ronni;" +
                @"emote-sets=0,33,50,237,793,2126,3517,4578,5569,9400,10337,12239;mod=1;subscriber=1;" +
                @"turbo=1;user-type=staff :tmi.twitch.tv USERSTATE #dallas";

            var _userstate = ReceivedMessage.Parse(USERSTATE);
            Assert.AreEqual(IrcMessageType.USERSTATE, _userstate.MessageType);
            if (_userstate is UserState userstate)
            {
                Assert.AreEqual("dallas", userstate.Channel);
                Assert.AreEqual(Color.FromArgb(13, 66, 0), userstate.Color);
                Assert.AreEqual("ronni", userstate.DisplayName);
                Assert.IsTrue(userstate.EmoteSets.SequenceEqual([0, 33, 50, 237, 793, 2126, 3517, 4578, 5569, 9400, 10337, 12239]));
                Assert.IsTrue(userstate.Moderator);
                Assert.IsTrue(userstate.Subscriber);
                Assert.IsTrue(userstate.Turbo);
                Assert.AreEqual(UserType.Staff, userstate.UserType);
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void TestWhisper()
        {
            //Taken from a Twitch documentation example
            //https://dev.twitch.tv/docs/irc/tags/#whisper-tags
            var WHISPER = @"@badges=staff/1,bits-charity/1;color=#8A2BE2;display-name=PetsgomOO;emotes=;message-id=306;" +
                @"thread-id=12345678_87654321;turbo=0;user-id=87654321;user-type=staff" +
                @" :petsgomoo!petsgomoo@petsgomoo.tmi.twitch.tv WHISPER foo :hello";

            var _whisper = ReceivedMessage.Parse(WHISPER);
            Assert.AreEqual(IrcMessageType.WHISPER, _whisper.MessageType);
            if (_whisper is Whisper whisper)
            {
                Assert.IsTrue(whisper.Badges.SequenceEqual([
                    new Badge("staff", "1"),
                    new Badge("bits-charity", "1"),
                    ]));
                Assert.AreEqual(Color.FromArgb(138, 43, 226), whisper.Color);
                Assert.AreEqual("PetsgomOO", whisper.DisplayName);
                Assert.IsTrue(whisper.Emotes.SequenceEqual([]));
                Assert.AreEqual("306", whisper.MessageId);
                Assert.AreEqual("12345678_87654321", whisper.ThreadId);
                Assert.IsFalse(whisper.Turbo);
                Assert.AreEqual("87654321", whisper.UserId);
                Assert.AreEqual(UserType.Staff, whisper.UserType);
                Assert.AreEqual("hello", whisper.Message);
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void TestGlobalUserState()
        {
            var GLOBAL = @"@badge-info=subscriber/8;badges=subscriber/6;color=#0D4200;display-name=dallas;" +
                @"emote-sets=0,33,50,237,793,2126,3517,4578,5569,9400,10337,12239;turbo=0;user-id=12345678;" +
                @"user-type=admin :tmi.twitch.tv GLOBALUSERSTATE";

            var _global = ReceivedMessage.Parse(GLOBAL);
            Assert.AreEqual(IrcMessageType.GLOBALUSERSTATE, _global.MessageType);
            if (_global is GlobalUserState global)
            {
                Assert.IsTrue(global.BadgeInfo.SequenceEqual(["subscriber/8"]));
                Assert.IsTrue(global.Badges.SequenceEqual([new Badge("subscriber", "6")]));
                Assert.AreEqual(Color.FromArgb(13, 66, 0), global.Color);
                Assert.AreEqual("dallas", global.DisplayName);
                Assert.IsTrue(global.EmoteSets.SequenceEqual([
                    0, 33, 50, 237, 793, 2126, 3517, 4578, 5569, 9400, 10337,
                    12239]));
                Assert.IsFalse(global.Turbo);
                Assert.AreEqual("12345678", global.UserId);
                Assert.AreEqual(UserType.Admin, global.UserType);
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void TestClearMsg()
        {
            var CLEARMSG = @"@login=ronni;room-id=;target-msg-id=abc-123-def;tmi-sent-ts=1642720582342" +
                @" :tmi.twitch.tv CLEARMSG #dallas :HeyGuys";

            var _clearmsg = ReceivedMessage.Parse(CLEARMSG);
            Assert.AreEqual(IrcMessageType.CLEARMSG, _clearmsg.MessageType);
            if (_clearmsg is ClearMsg clearmsg)
            {
                Assert.AreEqual("ronni", clearmsg.Login);
                Assert.AreEqual("", clearmsg.RoomId);
                Assert.AreEqual("abc-123-def", clearmsg.TargetMessageId);
                Assert.AreEqual(new DateTime(2022, 1, 20, 23, 16, 22, 342, DateTimeKind.Utc),
                    clearmsg.Timestamp);
                Assert.AreEqual("dallas", clearmsg.Channel);
                Assert.AreEqual("HeyGuys", clearmsg.Message);
            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void TestClearChat()
        {
            var PERMA = @"@room-id=12345678;target-user-id=87654321;tmi-sent-ts=1642715756806" +
                @" :tmi.twitch.tv CLEARCHAT #dallas :ronni";
            var CLEARCHAT = @"@room-id=12345678;tmi-sent-ts=1642715695392 :tmi.twitch.tv CLEARCHAT #dallas";
            var TIMEOUT = @"@ban-duration=350;room-id=12345678;target-user-id=87654321;tmi-sent-ts=1642719320727" +
                @" :tmi.twitch.tv CLEARCHAT #dallas :ronni";

            var _perma = ReceivedMessage.Parse(PERMA);
            Assert.AreEqual(IrcMessageType.CLEARCHAT, _perma.MessageType);
            if (_perma is ClearChat perma)
            {
                Assert.AreEqual("12345678", perma.RoomId);
                Assert.AreEqual("87654321", perma.TargetUserId);
                Assert.AreEqual(new DateTime(2022, 1, 20, 21, 55, 56, 806, DateTimeKind.Utc),
                    perma.Timestamp);
                Assert.AreEqual("dallas", perma.Channel);
                Assert.AreEqual("ronni", perma.User);
            }
            else
            {
                Assert.Fail();
            }

            var _clearchat = ReceivedMessage.Parse(CLEARCHAT);
            Assert.AreEqual(IrcMessageType.CLEARCHAT, _clearchat.MessageType);
            if (_clearchat is ClearChat clearchat)
            {
                Assert.AreEqual("12345678", clearchat.RoomId);
                Assert.AreEqual(new DateTime(2022, 1, 20, 21, 54, 55, 392),
                    clearchat.Timestamp);
                Assert.AreEqual("dallas", clearchat.Channel);
            }
            else
            {
                Assert.Fail();
            }

            var _timeout = ReceivedMessage.Parse(TIMEOUT);
            Assert.AreEqual(IrcMessageType.CLEARCHAT, _timeout.MessageType);
            if (_timeout is ClearChat timeout)
            {

            }
            else
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void TestHostTarget()
        {
            var START = @":tmi.twitch.tv HOSTTARGET #abc :xyz 10";
            var END = @":tmi.twitch.tv HOSTTARGET #abc :- 10";
            //this should be valid based on the Twitch documentation but there
            //doesn't seem to be a real use case
            var NOCHAN = @":tmi.twitch.tv HOSTTARGET #abc : 10";

            var _start = ReceivedMessage.Parse(START);
            Assert.AreEqual(IrcMessageType.HOSTTARGET, _start.MessageType);
            if (_start is HostTarget start)
            {
                Assert.AreEqual("abc", start.HostingChannel);
                Assert.AreEqual("xyz", start.ChannelBeingHosted);
                Assert.AreEqual(10, start.NumberOfViewers);
                Assert.IsTrue(start.NowHosting);
            }
            else
            {
                Assert.Fail();
            }

            var _end = ReceivedMessage.Parse(END);
            Assert.AreEqual(IrcMessageType.HOSTTARGET, _end.MessageType);
            if (_end is HostTarget end)
            {
                Assert.AreEqual("abc", end.HostingChannel);
                Assert.AreEqual("", end.ChannelBeingHosted);
                Assert.IsFalse(end.NowHosting);
                Assert.AreEqual(10, end.NumberOfViewers);
            }
            else
            {
                Assert.Fail();
            }

            var _nochan = ReceivedMessage.Parse(NOCHAN);
            Assert.AreEqual(IrcMessageType.HOSTTARGET, _nochan.MessageType);
            if (_nochan is HostTarget nochan)
            {
                Assert.AreEqual("abc", nochan.HostingChannel);
                Assert.AreEqual("", nochan.ChannelBeingHosted);
                Assert.IsTrue(nochan.NowHosting);
                Assert.AreEqual(10, nochan.NumberOfViewers);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}