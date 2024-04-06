using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using TwitchIrcClient.EventSub.Messages;

namespace TwitchIrcClientTests
{
    [TestClass]
    [DeploymentItem("EventSubExampleJson")]
    public class EventSubJsonTest
    {
        [TestMethod]
        public void TestEventSubNotification()
        {
            var text = File.ReadAllText("EventSubNotification_AutomodMessageHold.json");
            var automodMessageHold = EventSubMessage.Parse(text);
            Assert.IsInstanceOfType<EventSubNotification>(automodMessageHold);
            var esn = (EventSubNotification)automodMessageHold;
            Assert.AreEqual("befa7b53-d79d-478f-86b9-120f112b044e", esn.Metadata.MessageId);
            Assert.AreEqual(EventSubMessageType.Notification, esn.MessageType);
            //test accuracy to a millisecond
            Assert.AreEqual(new DateTime(2022, 11, 16, 10, 11, 12, 464, 578, DateTimeKind.Utc).Ticks,
                esn.Metadata.MessageTime.Ticks, 10000);
            Assert.AreEqual("automod.message.hold", esn.Metadata.SubscriptionType);
            Assert.AreEqual("1", esn.Metadata.SubscriptionVersion);
            Assert.AreEqual("f1c2a387-161a-49f9-a165-0f21d7a4e1c4", esn.Payload.Subscription.Id);
            Assert.AreEqual("automod.message.hold", esn.Payload.Subscription.Type);
            Assert.AreEqual("beta", esn.Payload.Subscription.Version);
            Assert.AreEqual("enabled", esn.Payload.Subscription.Status);
            Assert.AreEqual(0, esn.Payload.Subscription.Cost);
            Assert.AreEqual("websocket", esn.Payload.Subscription.Transport.Method);
            Assert.AreEqual("123456789", esn.Payload.Subscription.Transport.SessionId);
        }
        [TestMethod]
        public void TestEventSubWelcome()
        {
            var text = File.ReadAllText("EventSubWelcome.json");
            var welcome = EventSubMessage.Parse(text);
            Assert.IsInstanceOfType<EventSubWelcome>(welcome);
            var esw = (EventSubWelcome)welcome;
            Assert.AreEqual("96a3f3b5-5dec-4eed-908e-e11ee657416c", esw.Metadata.MessageId);
            Assert.AreEqual(EventSubMessageType.Welcome, esw.MessageType);
            Assert.AreEqual(new DateTime(2023, 7, 19, 14, 56, 51, 634, 234, DateTimeKind.Utc).Ticks,
                esw.Metadata.MessageTime.Ticks, 10000);
        }
        [TestMethod]
        public void TestEventSubRevocation()
        {
            var text = File.ReadAllText("EventSubRevocation_ChannelFollow.json");
            var channelFollow = EventSubMessage.Parse(text);
            Assert.IsInstanceOfType<EventSubRevocation>(channelFollow);
            var esr = (EventSubRevocation)channelFollow;
            Assert.AreEqual("84c1e79a-2a4b-4c13-ba0b-4312293e9308", esr.Metadata.MessageId);
            Assert.AreEqual(EventSubMessageType.Revocation, esr.MessageType);
            Assert.AreEqual(new DateTime(2022, 11, 16, 10, 11, 12, 464, 757, DateTimeKind.Utc).Ticks,
                esr.Metadata.MessageTime.Ticks, 10000);
            Assert.AreEqual("channel.follow", esr.Metadata.SubscriptionType);
            Assert.AreEqual("1", esr.Metadata.SubscriptionVersion);
            var sub = esr.Payload.Subscription;
            Assert.AreEqual("f1c2a387-161a-49f9-a165-0f21d7a4e1c4", sub.Id);
            Assert.AreEqual("authorization_revoked", sub.Status);
            Assert.AreEqual("channel.follow", sub.Type);
            Assert.AreEqual("1", sub.Version);
            Assert.AreEqual(1, sub.Cost);
            Assert.AreEqual("websocket", sub.Transport.Method);
            Assert.AreEqual("AQoQexAWVYKSTIu4ec_2VAxyuhAB", sub.Transport.SessionId);
            Assert.AreEqual(new DateTime(2022, 11, 16, 10, 11, 12, 464, 757, DateTimeKind.Utc).Ticks,
                esr.Payload.Subscription.CreatedAt.Ticks, 10000);
        }
        [TestMethod]
        public void TestEventSubReconnect()
        {
            var text = File.ReadAllText("EventSubReconnect.json");
            var reconnect = EventSubMessage.Parse(text);
            Assert.IsInstanceOfType<EventSubReconnect>(reconnect);
            var esr = (EventSubReconnect)reconnect;
            Assert.AreEqual("84c1e79a-2a4b-4c13-ba0b-4312293e9308", esr.Metadata.MessageId);
            Assert.AreEqual(EventSubMessageType.Reconnect, esr.MessageType);
            Assert.AreEqual(new DateTime(2022, 11, 18, 9, 10, 11, 634, 234, DateTimeKind.Utc).Ticks,
                esr.Metadata.MessageTime.Ticks, 10000);
            Assert.AreEqual("AQoQexAWVYKSTIu4ec_2VAxyuhAB", esr.Payload.Session.Id);
            Assert.AreEqual("reconnecting", esr.Payload.Session.Status);
            Assert.IsNull(esr.Payload.Session.KeepaliveTimeoutSeconds);
            Assert.AreEqual("wss://eventsub.wss.twitch.tv?...", esr.Payload.Session.ReconnectUrl);
            Assert.AreEqual(new DateTime(2022, 11, 16, 10, 11, 12, 634, 234, DateTimeKind.Utc).Ticks,
                esr.Payload.Session.ConnectedAt.Ticks, 10000);
        }
        [TestMethod]
        public void TestEventSubKeepalive()
        {
            var text = File.ReadAllText("EventSubKeepalive.json");
            var keepalive = EventSubMessage.Parse(text);
            Assert.IsInstanceOfType<EventSubKeepalive>(keepalive);
            var esk = (EventSubKeepalive)keepalive;
            Assert.AreEqual("84c1e79a-2a4b-4c13-ba0b-4312293e9308", esk.Metadata.MessageId);
            Assert.AreEqual(EventSubMessageType.Keepalive, esk.MessageType);
            Assert.AreEqual(new DateTime(2023, 7, 19, 10, 11, 12, 634, 234, DateTimeKind.Utc).Ticks,
                esk.Metadata.MessageTime.Ticks, 10000);
            Assert.IsFalse(esk.Payload.Any());
        }
    }
}
