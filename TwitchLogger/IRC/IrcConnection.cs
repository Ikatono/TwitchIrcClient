using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace TwitchLogger.IRC
{
    /// <summary>
    /// Connects to a single Twitch chat channel via limited IRC implementation.
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="port"></param>
    public class IrcConnection(string url, int port) : IDisposable
    {
        public static readonly string ENDL = "\r\n";
        public int Port { get; } = port;
        public string Url { get; } = url;
        public bool Connected { get; } = false;

        public event EventHandler? onTimeout;

        private TcpClient Client = new();
        private NetworkStream Stream => Client.GetStream();
        private CancellationTokenSource TokenSource = new();
        private RateLimiter? Limiter;
        private Task? ListenerTask;

        public async Task<bool> ConnectAsync()
        {
            if (Connected)
                return true;
            Client.NoDelay = true;
            await Client.ConnectAsync(Url, Port);
            if (!Client.Connected)
                return false;
            ListenerTask = Task.Run(() => ListenForInput(TokenSource.Token), TokenSource.Token);
            return true;
        }
        public void Disconnect()
        {
            TokenSource.Cancel();
        }
        public void SendLine(string line)
        {
            Limiter?.WaitForAvailable();
            if (TokenSource.IsCancellationRequested)
                return;
            Stream.Write(new Span<byte>(Encoding.UTF8.GetBytes(line + ENDL)));
        }
        public void Authenticate(string user, string pass)
        {
            SendLine($"NICK {user}");
            SendLine($"PASS {pass}");
        }
        private async void ListenForInput(CancellationToken token)
        {
            using AutoResetEvent ARE = new(false);
            byte[] buffer = new byte[5 * 1024];
            while (!token.IsCancellationRequested)
            {
                var bytesRead = await Stream.ReadAsync(buffer, 0, buffer.Length, TokenSource.Token);
                if (bytesRead > 0)
                    onDataReceived(buffer, bytesRead);
                if (!Stream.CanRead)
                    return;
            }
            token.ThrowIfCancellationRequested();
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
                SendLine($"PONG :{message.Source} {message.RawParameters}");
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

        private List<CallbackItem> Callbacks = [];
        public void AddCallback(CallbackItem callbackItem)
            => Callbacks.Add(callbackItem);
        public bool RemoveCallback(CallbackItem callbackItem)
            => Callbacks.Remove(callbackItem);
        private void RunCallbacks(ReceivedMessage message)
        {
            ArgumentNullException.ThrowIfNull(message, nameof(message));
            Callbacks.ForEach(c => c.TryCall(message));
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
                    Limiter?.Dispose();
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

        private class RateLimiter : IDisposable
        {
            private SemaphoreSlim Semaphore;
            private System.Timers.Timer Timer;
            public int MessageLimit { get; }
            public int Seconds { get; }
            private CancellationToken Token { get; }
            
            public RateLimiter(int messages, int seconds, CancellationToken token)
            {
                Semaphore = new(messages, messages);
                Timer = new(TimeSpan.FromSeconds(seconds));
                MessageLimit = messages;
                Seconds = seconds;
                Token = token;
                Timer.AutoReset = true;
                Timer.Elapsed += ResetLimit;
                Timer.Start();
            }

            public void WaitForAvailable()
            {
                try
                {
                    Semaphore.Wait(Token);
                }
                catch (OperationCanceledException)
                {
                    //caller is responsible for checking whether connection is cancelled before trying to send
                }
            }
            public bool WaitForAvailable(TimeSpan timeout)
            {
                try
                {
                    return Semaphore.Wait(timeout, Token);
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
            }
            public bool WaitForAvailable(int millis)
            {
                try
                {
                    return Semaphore.Wait(millis, Token);
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
            }

            private void ResetLimit(object? sender, EventArgs e)
            {
                try
                {
                    Semaphore.Release(MessageLimit);
                }
                catch (SemaphoreFullException)
                {

                }
                catch (ObjectDisposedException)
                {

                }
            }

            #region RateLimiter Dispose
            private bool disposedValue;
            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        Semaphore?.Dispose();
                        Timer?.Dispose();
                    }
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
            #endregion //RateLimiter Dispose
        }
    }
}
