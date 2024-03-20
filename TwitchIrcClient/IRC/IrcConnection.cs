using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TwitchIrcClient.IRC.Messages;
using TwitchLogger.IRC.Messages;

namespace TwitchLogger.IRC
{
    /// <summary>
    /// Connects to a single Twitch chat channel via limited IRC implementation.
    /// </summary>
    public class IrcConnection : IDisposable
    {
        public static readonly string ENDL = "\r\n";
        public int Port { get; }
        public string Url { get; }
        public bool Connected { get; } = false;
        public bool TrackUsers { get; }
        public bool UsesSsl { get; }
        private Roomstate? _LastRoomstate;
        public Roomstate? LastRoomstate
        { get
            {
                if (_LastRoomstate == null)
                    return null;
                return new Roomstate(_LastRoomstate);
            }
        }
        //this seems to be the only concurrentcollection that allows
        //removing specific items
        protected ConcurrentDictionary<string, byte> UserCollection = new();
        public IEnumerable<string> Users => UserCollection.Keys;

        public event EventHandler? onTimeout;
        /// <summary>
        /// Occassionally sends a list of users who have joined and left the server.
        /// Twitch sends this in bulk, so this event tries to collect all of these
        /// into one call. Only reacts to users who join through 
        /// </summary>
        public event EventHandler<UserChangeEventArgs>? onUserChange;

        private TcpClient Client = new();
        //private NetworkStream Stream => Client.GetStream();
        private Stream _Stream;
        private CancellationTokenSource TokenSource = new();
        //it looks like you can't get the Token after the Source is disposed
        protected CancellationToken Token;
        private RateLimiter? Limiter;
        private Task? ListenerTask;
        private Task? UserUpdateTask;

        public IrcConnection(string url, int port,
            RateLimiter? limiter = null, bool trackUsers = false, bool useSsl = false)
        {
            Url = url;
            Port = port;
            Limiter = limiter;
            TrackUsers = trackUsers;
            UsesSsl = useSsl;
            Token = TokenSource.Token;
            if (TrackUsers)
            {
                AddSystemCallback(new MessageCallbackItem((o, m) =>
                {
                    if (m is NamReply nr)
                        foreach (var u in nr.Users)
                            o.UserCollection.TryAdd(u, 0);
                    else
                        throw new ArgumentException(null, nameof(m));
                }, [IrcMessageType.RPL_NAMREPLY]));
                AddSystemCallback(new MessageCallbackItem((o, m) =>
                {
                    if (m is Join j)
                    {
                        o.UserCollection.TryAdd(j.Username, 0);
                        o.UserJoin(j);
                    }
                    else
                        throw new ArgumentException(null, nameof(m));
                }, [IrcMessageType.JOIN]));
                AddSystemCallback(new MessageCallbackItem((o, m) =>
                {
                    if (m is Part j)
                    {
                        o.UserCollection.TryRemove(j.Username, out _);
                        o.UserLeave(j);
                    }
                    else
                        throw new ArgumentException(null, nameof(m));
                }, [IrcMessageType.PART]));
            }
            AddSystemCallback(new MessageCallbackItem(
                (o, m) => { o._LastRoomstate = new Roomstate(m); },
                [IrcMessageType.ROOMSTATE]));
        }

        public async Task<bool> ConnectAsync()
        {
            if (Connected)
                return true;
            Client.NoDelay = true;
            await Client.ConnectAsync(Url, Port);
            if (!Client.Connected)
                return false;
            if (UsesSsl)
            {
                var stream = new SslStream(Client.GetStream());
                await stream.AuthenticateAsClientAsync(Url);
                _Stream = stream;
            }
            else
            {
                _Stream = Client.GetStream();
            }
            ListenerTask = Task.Run(ListenForInput, Token);
            UserUpdateTask = Task.Run(UpdateUsers, Token);
            return true;
        }
        public void Disconnect()
        {
            TokenSource.Cancel();
        }
        public void SendLine(string line)
        {
            Limiter?.WaitForAvailable(Token);
            if (Token.IsCancellationRequested)
                return;
            var bytes = Encoding.UTF8.GetBytes(line + ENDL);
            _Stream.Write(bytes, 0, bytes.Length);
        }
        public void Authenticate(string? user, string? pass)
        {
            if (user == null)
                user = $"justinfan{Random.Shared.NextInt64(10000):D4}";
            if (pass == null)
                pass = "pass";
            SendLine($"NICK {user}");
            SendLine($"PASS {pass}");
        }
        public void JoinChannel(string channel)
        {
            channel = channel.TrimStart('#');
            SendLine($"JOIN #{channel}");
        }
        private async void ListenForInput()
        {
            using AutoResetEvent ARE = new(false);
            byte[] buffer = new byte[5 * 1024];
            while (!Token.IsCancellationRequested)
            {
                var bytesRead = await _Stream.ReadAsync(buffer, Token);
                if (bytesRead > 0)
                    onDataReceived(buffer, bytesRead);
                if (!_Stream.CanRead)
                    return;
            }
            Token.ThrowIfCancellationRequested();
        }

        private readonly ConcurrentBag<string> _JoinedUsers = [];
        private readonly ConcurrentBag<string> _LeftUsers = [];
        private void UserJoin(Join message)
        {
            _JoinedUsers.Add(message.Username);
        }
        private void UserLeave(Part message)
        {
            _LeftUsers.Add(message.Username);
        }
        private async void UpdateUsers()
        {
            while (true)
            {
                List<string> join = [];
                List<string> leave = [];
                var args = new UserChangeEventArgs(join, leave);
                await Task.Delay(2000, Token);
                if (Token.IsCancellationRequested)
                    return;
                //poll the collections to see if they have items
                while (true)
                {
                    if (_JoinedUsers.TryTake(out string joinUser))
                    {
                        join.Add(joinUser);
                        break;
                    }
                    if (_LeftUsers.TryTake(out string leaveUser))
                    {
                        leave.Add(leaveUser);
                        break;
                    }
                    await Task.Delay(500, Token);
                    if (Token.IsCancellationRequested)
                        return;
                }
                //once and item is found, wait a bit for Twitch to send the others
                await Task.Delay(2000, TokenSource.Token);
                if (TokenSource.IsCancellationRequested)
                    return;
                while (_JoinedUsers.TryTake(out string user))
                    join.Add(user);
                while (_LeftUsers.TryTake(out string user))
                    leave.Add(user);
                onUserChange?.Invoke(this, args);
            }
        }
        private string _ReceivedDataBuffer = "";
        private void onDataReceived(byte[] buffer, int length)
        {
            string receivedString = Encoding.UTF8.GetString(buffer, 0, length);
            _ReceivedDataBuffer += receivedString;
            string[] lines = _ReceivedDataBuffer.Split(ENDL);
            //if last line is terminated, there should be an empty string at the end of "lines"
            foreach (var line in lines.SkipLast(1))
                onLineReceived(line);
            _ReceivedDataBuffer = lines.Last();
        }
        private void onLineReceived(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return;
            var message = ReceivedMessage.Parse(line);
            HeartbeatReceived();
            //PONG must be sent automatically
            if (message.MessageType == IrcMessageType.PING)
                SendLine(message.RawText.Replace("PING", "PONG"));
            RunCallbacks(message);
        }
        //TODO consider changing to a System.Threading.Timer, I'm not sure
        //if it's a better fit
        private readonly System.Timers.Timer _HeartbeatTimer = new();
        private void InitializeHeartbeat(int millis)
        {
            ObjectDisposedException.ThrowIf(disposedValue, GetType());
            _HeartbeatTimer.AutoReset = false;
            _HeartbeatTimer.Interval = millis;
            _HeartbeatTimer.Elapsed += HeartbeatTimedOut;
            _HeartbeatTimer.Start();
        }
        private void HeartbeatReceived()
        {
            if (disposedValue)
                return;
            _HeartbeatTimer.Stop();
            _HeartbeatTimer.Start();
        }
        private void HeartbeatTimedOut(object? caller, ElapsedEventArgs e)
        {
            if (disposedValue)
                return;
            onTimeout?.Invoke(this, EventArgs.Empty);
        }

        private readonly List<MessageCallbackItem> UserCallbacks = [];
        protected readonly List<MessageCallbackItem> SystemCallbacks = [];
        public void AddCallback(MessageCallbackItem callbackItem)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            UserCallbacks.Add(callbackItem);
        }
        public bool RemoveCallback(MessageCallbackItem callbackItem)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return UserCallbacks.Remove(callbackItem);
        }
        protected void AddSystemCallback(MessageCallbackItem callbackItem)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            SystemCallbacks.Add(callbackItem);
        }
        protected bool RemoveSystemCallback(MessageCallbackItem callbackItem)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return SystemCallbacks.Remove(callbackItem);
        }
        private void RunCallbacks(ReceivedMessage message)
        {
            ArgumentNullException.ThrowIfNull(message, nameof(message));
                if (disposedValue)
                return;
            SystemCallbacks.ForEach(c => c.TryCall(this, message));
            UserCallbacks.ForEach(c => c.TryCall(this, message));
        }

        #region Dispose
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                TokenSource.Cancel();
                if (disposing)
                {
                    TokenSource.Dispose();
                    Client?.Dispose();
                    _HeartbeatTimer?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion //Dispose
    }
}
