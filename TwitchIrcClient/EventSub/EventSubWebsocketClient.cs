using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using TwitchIrcClient.IRC.Messages;
using TwitchIrcClient.IRC;
using TwitchIrcClient.EventSub.Messages;

namespace TwitchIrcClient.EventSub
{
    public class EventSubWebsocketClient : IDisposable
    {
        private readonly ClientWebSocket Socket = new();
        private readonly CancellationTokenSource TokenSource = new();
        public CancellationToken CancelToken => TokenSource.Token;
        public readonly HttpClient Client = new();
        public string? SessionId { get; private set; }
        public string? ReconnectUrl { get; private set; }
        public int? TotalSubscriptions { get; internal set; }
        public int? TotalCost { get; internal set; }
        public int? MaximumCost { get; internal set; }

        public async Task<bool> ConnectAsync()
        {
            const string url = "wss://eventsub.wss.twitch.tv/ws?keepalive_timeout_seconds=600";
            await Socket.ConnectAsync(new Uri(url), CancelToken);
            if (CancelToken.IsCancellationRequested)
                return false;
            if (Socket.State != WebSocketState.Open)
                return false;
            System_ReceivedWelcome += (sender, e) =>
            {
                if (sender is EventSubWebsocketClient esc)
                {
                    esc.SessionId = e.Welcome.Payload.Session.Id;
                    esc.ReconnectUrl = e.Welcome.Payload.Session.ReconnectUrl;
                }
            };
            _ = Task.Run(HandleIncomingMessages, CancelToken);
            return true;
        }

        private async Task HandleIncomingMessages()
        {
            var arr = new byte[8 * 1024];
            var buffer = new List<byte>();
            Task? prevTask = null;
            while (true)
            {
                var resp = await Socket.ReceiveAsync(arr, CancelToken);
                if (CancelToken.IsCancellationRequested)
                    return;
                buffer.AddRange(arr.Take(resp.Count));
                if (resp.EndOfMessage)
                {
                    var str = Encoding.UTF8.GetString(buffer.ToArray());
                    //events get their own task so future messages aren't delayed
                    //use ContinueWith to preserve otder of incoming messages
                    prevTask = prevTask?.IsCompleted ?? true
                        ? Task.Run(() => DoIncomingMessage(str), CancelToken)
                        : prevTask.ContinueWith((_, _) => DoIncomingMessage(str), CancelToken);
                    buffer.Clear();
                }
            }
        }

        private event EventHandler<EventSubKeepaliveEventArgs>? System_ReceivedKeepalive;
        private event EventHandler<EventSubNotificationEventArgs>? System_ReceivedNotification;
        private event EventHandler<EventSubReconnectEventArgs>? System_ReceivedReconnect;
        private event EventHandler<EventSubRevocationEventArgs>? System_ReceivedRevocation;
        private event EventHandler<EventSubWelcomeEventArgs>? System_ReceivedWelcome;

        public event EventHandler<EventSubKeepaliveEventArgs>? ReceivedKeepalive;
        public event EventHandler<EventSubNotificationEventArgs>? ReceivedNotification;
        public event EventHandler<EventSubReconnectEventArgs>? ReceivedReconnect;
        public event EventHandler<EventSubRevocationEventArgs>? ReceivedRevocation;
        public event EventHandler<EventSubWelcomeEventArgs>? ReceivedWelcome;
        private void DoIncomingMessage(string message)
        {
            var esm = EventSubMessage.Parse(message);
            switch (esm)
            {
                case EventSubKeepalive keepalive:
                    System_ReceivedKeepalive?.Invoke(this, new EventSubKeepaliveEventArgs(keepalive));
                    ReceivedKeepalive?.Invoke(this, new EventSubKeepaliveEventArgs(keepalive));
                    break;
                case EventSubNotification notification:
                    System_ReceivedNotification?.Invoke(this, new EventSubNotificationEventArgs(notification));
                    ReceivedNotification?.Invoke(this, new EventSubNotificationEventArgs(notification));
                    break;
                case EventSubReconnect reconnect:
                    System_ReceivedReconnect?.Invoke(this, new EventSubReconnectEventArgs(reconnect));
                    ReceivedReconnect?.Invoke(this, new EventSubReconnectEventArgs(reconnect));
                    break;
                case EventSubRevocation revocation:
                    System_ReceivedRevocation?.Invoke(this, new EventSubRevocationEventArgs(revocation));
                    ReceivedRevocation?.Invoke(this, new EventSubRevocationEventArgs(revocation));
                    break;
                case EventSubWelcome welcome:
                    System_ReceivedWelcome?.Invoke(this, new EventSubWelcomeEventArgs(welcome));
                    ReceivedWelcome?.Invoke(this, new EventSubWelcomeEventArgs(welcome));
                    break;
                default:
                    throw new InvalidDataException("invalid message type");
            }
        }

        #region dispose
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                TokenSource.Cancel();
                if (disposing)
                {
                    Socket?.Dispose();
                    Client?.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion //dispose
    }
}
