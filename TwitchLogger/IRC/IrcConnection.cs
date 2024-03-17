using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace TwitchLogger.IRC
{
    public class IrcConnection : IDisposable
    {
        public static readonly string ENDL = "\r\n";
        public int Port { get; }
        public string Url { get; }
        public bool Connected { get; } = false;

        public event EventHandler? onTimeout;

        private Socket Socket = new(SocketType.Stream, ProtocolType.Tcp);
        private CancellationTokenSource CancellationTokenSource = new();
        private Thread? ListenerThread;

        public IrcConnection(string url, int port)
        {
            Url = url;
            Port = port;
        }
        public async Task<bool> ConnectAsync()
        {
            if (Connected)
                return true;
            await Socket.ConnectAsync(Url, Port);
            if (!Socket.Connected)
                return false;
            ListenerThread = new(() => ListenForInput(CancellationTokenSource.Token));
            ListenerThread.Start();
            return true;
        }
        public void Disconnect()
        {
            CancellationTokenSource.Cancel();
            throw new NotImplementedException();
        }
        public void SendLine(string line)
        {
            int sent = Socket.Send(Encoding.UTF8.GetBytes(line + ENDL));
        }
        public bool Authenticate(string user, string pass)
        {
            throw new NotImplementedException();
        }
        private void ListenForInput(CancellationToken token)
        {
            using AutoResetEvent ARE = new(false);
            while (true)
            {
                SocketAsyncEventArgs args = new();
                args.Completed += (sender, e) =>
                {
                    onDataReceived(e);
                    ARE.Set();
                };
                bool started = Socket.ReceiveAsync(args);
                while (true)
                {
                    bool reset = ARE.WaitOne(100);
                    if (reset)
                        break;
                    //returning ends the thread running this
                    if (token.IsCancellationRequested)
                        return;
                }
            }
        }
        private string _ReceivedDataBuffer = "";
        private void onDataReceived(SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success)
                throw new SocketException((int)args.SocketError, $"Socket Error: {args.SocketError}");
            if (args.Buffer is null)
                throw new ArgumentNullException();
            string receivedString = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
            _ReceivedDataBuffer += receivedString;
            string[] lines = _ReceivedDataBuffer.Split(ENDL);
            //if last line is terminated, there should be an empty string at the end of "lines"
            foreach (var line in lines.SkipLast(1))
                onLineReceived(line);
            _ReceivedDataBuffer = lines.Last();
        }
        private void onLineReceived(string line)
        {
            throw new NotImplementedException();
        }
        private System.Timers.Timer _HeartbeatTimer = new();
        private void InitializeHeartbeat(int millis)
        {
            _HeartbeatTimer.AutoReset = false;
            _HeartbeatTimer.Interval = millis;
            _HeartbeatTimer.Elapsed += HeartbeatTimedOut;
        }
        private void HeartbeatReceived()
        {
            throw new NotImplementedException();
        }
        private void HeartbeatTimedOut(object? caller, ElapsedEventArgs e)
        {
            onTimeout?.Invoke(this, new());
        }

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                CancellationTokenSource.Cancel();
                if (disposing)
                {
                    Socket?.Dispose();
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
    }
}
