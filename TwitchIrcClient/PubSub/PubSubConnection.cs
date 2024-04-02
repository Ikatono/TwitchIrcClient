using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web;
using TwitchIrcClient.PubSub.Message;

namespace TwitchIrcClient.PubSub
{
    public sealed class PubSubConnection : IDisposable
    {
        //private TcpClient Client = new();
        //private SslStream SslStream;
        //private WebSocket Socket;
        private ClientWebSocket Socket = new();
        private CancellationTokenSource TokenSource = new();
        private string? ClientId;
        private string? AuthToken;
        private DateTime? AuthExpiration;

        public string RefreshToken { get; private set; }

        public PubSubConnection()
        {

        }
        //this needs to be locked for thread-safety
        public async Task SendMessageAsync(string message)
        {
            await Socket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text,
                WebSocketMessageFlags.EndOfMessage | WebSocketMessageFlags.DisableCompression,
                TokenSource.Token);
        }
        public async Task SendMessageAsync(PubSubMessage message)
        {
            await SendMessageAsync(message.Serialize());
        }
        public async Task<bool> ConnectAsync()
        {
            const string url = "wss://pubsub-edge.twitch.tv";
            await Socket.ConnectAsync(new Uri(url), TokenSource.Token);
            if (Socket.State != WebSocketState.Open)
                return false;
            _ = Task.Run(HandlePings, TokenSource.Token);
            _ = Task.Run(HandleIncomingMessages, TokenSource.Token);
            return true;
        }
        public async Task<bool> GetImplicitTokenAsync(string clientId, string clientSecret,
            IEnumerable<string> scopes)
        {
            const int PORT = 17563;
            using var listener = new TcpListener(System.Net.IPAddress.Any, PORT);
            listener.Start();
            //using var client = new HttpClient();
            var scopeString = string.Join(' ', scopes);
            var stateNonce = MakeNonce();
            var url = $"https://id.twitch.tv/oauth2/authorize" +
                $"?response_type=code" +
                $"&client_id={HttpUtility.UrlEncode(clientId)}" +
                $"&redirect_uri=http://localhost:{PORT}" +
                $"&scope={HttpUtility.UrlEncode(scopeString)}" +
                $"&state={stateNonce}";
            var startInfo = new ProcessStartInfo()
            {
                //FileName = "explorer",
                //Arguments = url,
                FileName = url,
                UseShellExecute = true,
            };
            Process.Start(startInfo);
            //Console.WriteLine(url);
            using var socket = await listener.AcceptSocketAsync(TokenSource.Token);
            var arr = new byte[2048];
            var buffer = new ArraySegment<byte>(arr);
            var count = await socket.ReceiveAsync(buffer, TokenSource.Token);
            var http204 = "HTTP/1.1 204 No Content\r\n\r\n";
            var sentCount = await socket.SendAsync(Encoding.UTF8.GetBytes(http204));
            var resp = Encoding.UTF8.GetString(arr, 0, count);
            var dict =
                //get the first line of HTTP response
                HttpUtility.UrlDecode(resp.Split("\r\n").First()
                //extract location component (trim leading /?)
                .Split(' ').ElementAt(1)[2..])
                //make a dictionary
                .Split('&').Select(s =>
                {
                    var p = s.Split('=');
                    return new KeyValuePair<string, string>(p[0], p[1]);
                }).ToDictionary();
            if (dict["state"] != stateNonce)
                return false;
            var payload = DictToBody(new Dictionary<string,string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["code"] = dict["code"],
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = $"http://localhost:{PORT}",
            });
            var client = new HttpClient();
            var startTime = DateTime.Now;
            var httpResp = await client.PostAsync("https://id.twitch.tv/oauth2/token",
                new StringContent(payload, new MediaTypeHeaderValue("application/x-www-form-urlencoded")));
            if (httpResp is null)
                return false;
            if (httpResp.Content is null)
                return false;
            if (!httpResp.IsSuccessStatusCode)
                return false;
            var respStr = await httpResp.Content.ReadAsStringAsync();
            var json = JsonNode.Parse(respStr);
            string authToken;
            double expiresIn;
            string refreshToken;
            if ((json?["access_token"]?.AsValue().TryGetValue(out authToken) ?? false)
                && (json?["expires_in"]?.AsValue().TryGetValue(out expiresIn) ?? false)
                && (json?["refresh_token"]?.AsValue().TryGetValue(out refreshToken) ?? false))
            {
                AuthToken = authToken;
                RefreshToken = refreshToken;
                AuthExpiration = startTime.AddSeconds(expiresIn);
                ClientId = clientId;
                return true;
            }
            return false;
        }
        private static string DictToBody(IEnumerable<KeyValuePair<string,string>> dict)
        {
            return string.Join('&', dict.Select(p =>
                HttpUtility.UrlEncode(p.Key) + '=' + HttpUtility.UrlEncode(p.Value)));
        }
        public async Task<bool> GetDcfTokenAsync(string clientId, IEnumerable<string> scopes)
        {
            using var client = new HttpClient();
            var scopeString = string.Join(',', scopes);
            var startTime = DateTime.Now;
            var resp = await client.PostAsync("https://id.twitch.tv/oauth2/device",
                new StringContent($"client_id={clientId}&scopes={scopeString}",
                MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")));
            if (resp is null)
                return false;
            if (!resp.IsSuccessStatusCode)
                return false;
            if (resp.Content is null)
                return false;
            var contentString = await resp.Content.ReadAsStringAsync();
            var json = JsonNode.Parse(contentString);
            if (json is null)
                return false;
            throw new NotImplementedException();
        }
        public async Task<bool> GetTokenAsync(string clientId, string clientSecret)
        {
            using var client = new HttpClient();
            var startTime = DateTime.Now;
            var resp = await client.PostAsync($"https://id.twitch.tv/oauth2/token" +
                $"?client_id={clientId}&client_secret={clientSecret}" +
                $"&grant_type=client_credentials", null);
            if (resp is null)
                return false;
            if (!resp.IsSuccessStatusCode)
                return false;
            if (resp.Content is null)
                return false;
            var json = JsonNode.Parse(await resp.Content.ReadAsStringAsync());
            if (json is null)
                return false;
            var authToken = json["access_token"]?.GetValue<string>();
            var expiresIn = json["expires_in"]?.GetValue<double>();
            if (authToken is string token && expiresIn is double expires)
            {
                ClientId = clientId;
                AuthToken = token;
                AuthExpiration = startTime.AddSeconds(expires);
            }
            else
                return false;
            return true;
        }
        public async Task SubscribeAsync(IEnumerable<string> topics)
        {
            var psm = new PubSubMessage
            {
                ["type"] = "LISTEN",
                ["data"] = new JsonObject
                {
                    //TODO there's probably a cleaner way to do this
                    ["topics"] = new JsonArray(topics.Select(t => (JsonValue)t).ToArray()),
                    ["auth_token"] = AuthToken,
                },
                ["nonce"] = MakeNonce(),
            };
            await SendMessageAsync(psm);
        }
        //TODO change or dupe this to get multiple at once
        public async Task<string?> GetChannelIdFromNameAsync(string channelName)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AuthToken}");
            client.DefaultRequestHeaders.Add("Client-Id", ClientId);
            var resp = await client.GetAsync($"https://api.twitch.tv/helix/users?login={channelName}");
            if (resp is null)
                return null;
            if (!resp.IsSuccessStatusCode)
                return null;
            if (resp.Content is null)
                return null;
            var json = JsonNode.Parse(await resp.Content.ReadAsStringAsync());
            if (json is null)
                return null;
            var arr = json["data"];
            if (arr is null)
                return null;
            JsonArray jarr;
            try
            {
                jarr = arr.AsArray();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            var item = jarr.SingleOrDefault();
            if (item is null)
                return null;
            return item["id"]?.ToString();
        }
        private static string MakeNonce(int length = 16)
        {
            var buffer = new byte[length * 2];
            Random.Shared.NextBytes(buffer);
            return Convert.ToHexString(buffer);
        }
        private AutoResetEvent PingResetEvent = new(false);
        private async Task HandlePings()
        {
            //send ping every <5 minutes
            //wait until pong or >10 seconds
            //raise error if necessary
            AddSystemCallback(new PubSubCallbackItem(
                (m, s) =>
                {
                    s.PingResetEvent.Set();
                }, ["PONG"]));
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(4 * Jitter(0.05)));
                await SendMessageAsync(PubSubMessage.PING());
                await Task.Delay(TimeSpan.FromSeconds(10));
                if (!PingResetEvent.WaitOne(0))
                {
                    //timeout
                }
            }
        }
        private async void HandleIncomingMessages()
        {
            string s = "";
            while (true)
            {
                var buffer = new ArraySegment<byte>(new byte[4096]);
                var result = await Socket.ReceiveAsync(buffer, TokenSource.Token);
                s += Encoding.UTF8.GetString(buffer.Take(result.Count).ToArray());
                if (result.EndOfMessage)
                {
                    IncomingMessage(PubSubMessage.Parse(s));
                    s = "";
                }
            }
        }
        private void IncomingMessage(PubSubMessage message)
        {
            RunCallbacks(message);
        }
        private void RunCallbacks(PubSubMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);
            if (disposedValue)
                return;
            lock (SystemCallbacks)
                SystemCallbacks.ForEach(c => c.MaybeRunCallback(message, this));
            lock (UserCallbacks)
                UserCallbacks.ForEach(c => c.MaybeRunCallback(message, this));
        }
        private readonly List<PubSubCallbackItem> UserCallbacks = [];
        public void AddCallback(PubSubCallbackItem callback)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            lock (UserCallbacks)
                UserCallbacks.Add(callback);
        }
        public bool RemoveCallback(PubSubCallbackItem callback)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            lock (UserCallbacks)
                return UserCallbacks.Remove(callback);
        }
        private readonly List<PubSubCallbackItem> SystemCallbacks = [];
        private void AddSystemCallback(PubSubCallbackItem callback)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            lock (SystemCallbacks)
                SystemCallbacks.Add(callback);
        }
        private bool RemoveSystemCallback(PubSubCallbackItem callback)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            lock (SystemCallbacks)
                return SystemCallbacks.Remove(callback);
        }
        /// <summary>
        /// produces a number between -limit and limit
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        private static double Jitter(double limit)
        {
            return (Random.Shared.NextDouble() - 0.5) * 2 * limit;
        }
        #region Dispose
        private bool disposedValue;
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //Client?.Dispose();
                    //SslStream?.Dispose();
                    Socket?.Dispose();
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
