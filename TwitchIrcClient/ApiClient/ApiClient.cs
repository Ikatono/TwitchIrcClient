using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TwitchIrcClient.ApiClient.Messages;
using TwitchIrcClient.Authentication;
using TwitchIrcClient.EventSub;

namespace TwitchIrcClient.ApiClient
{
    public class ApiClient : IDisposable
    {
        private readonly HttpClient Client = new();
        private readonly CancellationTokenSource TokenSource = new();
        public CancellationToken CancellationToken => TokenSource.Token;
        public string? ClientId { get; set; }

        public async Task<EventSubResponse?> CreateWebsocketSubscriptionAsync(EventSubWebsocketClient eswClient,
            UserAccessAuthentication auth, string type, string version,
            IDictionary<string, string> condition)
        {
            var req = new EventSubRequest(type, version, condition,
                ApiTransport.MakeForWebsocket(
                    eswClient.SessionId ?? throw new InvalidOperationException(
                        "no session id, did websocket connection fail?")));
            using var content = JsonContent.Create(req);
            content.Headers.Add("Authorization", $"Bearer: {auth.Token}");
            content.Headers.Add("Client-Id", auth.ClientId);
            using var resp = await Client.PostAsync("https://api.twitch.tv/helix/eventsub/subscriptions",
                content, CancellationToken);
            if (!resp.IsSuccessStatusCode)
                return null;
            var j_resp = JsonSerializer.Deserialize<EventSubResponse>(await resp.Content.ReadAsStringAsync());
            if (j_resp is null)
                return null;
            eswClient.TotalCost = j_resp.TotalCost;
            eswClient.TotalSubscriptions = j_resp.Total;
            eswClient.MaximumCost = j_resp.MaxTotalCost;
            return j_resp;
        }
        public async Task<bool> DeleteWebSocketSubscriptionAsync(UserAccessAuthentication auth, string id)
        {
            using var req = new HttpRequestMessage();
            req.RequestUri = new Uri($"https://api.twitch.tv/helix/eventsub/subscriptions?id={id}");
            req.Headers.Add("Authorization", $"Bearer: {auth.Token}");
            req.Headers.Add("Client-Id", auth.ClientId);
            var resp = await Client.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }
        public async IAsyncEnumerable<EventSubResponseItem> GetWebsocketSubscriptionsAsync(
            UserAccessAuthentication auth, EventSubSubscriptionStatus? status = null,
            string? type = null, string? userId = null)
        {
            var attrs = new Dictionary<string, string>();
            if (status is EventSubSubscriptionStatus _status)
                attrs["status"] = EventSubSubscriptionStatusConverter.Convert(_status);
            if (type is string _type)
                attrs["type"] = _type;
            if (userId is string _userId)
                attrs["status"] = _userId;
            if (attrs.Count >= 2)
                throw new ArgumentException("cannot set more that 1 filter parameter");
            while (true)
            {
                using var req = new HttpRequestMessage();
                req.RequestUri = new Uri("https://api.twitch.tv/helix/eventsub/subscriptions?"
                    + string.Join(',', attrs.Select(p => $"{p.Key}={p.Value}")));
                req.Headers.Add("Authorization", $"Bearer {auth.Token}");
                req.Headers.Add("Client-Id", auth.ClientId);
                using var resp = await Client.SendAsync(req, CancellationToken);
                if (CancellationToken.IsCancellationRequested || resp is null)
                    yield break;
                if (!resp.IsSuccessStatusCode)
                    yield break;
                var esslr = JsonSerializer.Deserialize<EventSubSubscriptionListResponse>(
                    await resp.Content.ReadAsStringAsync());
                if (esslr is null)
                    yield break;
                foreach (var item in esslr.Data)
                    yield return item;
                if (esslr.Pagination is EventSubSubscriptionListResponsePagination pagination)
                    attrs["cursor"] = pagination.Cursor;
                else
                    yield break;
            }
        }
        public async Task<DcfCodeMessage?> GetDcfTokenAsync(string clientId,
            IEnumerable<string> scopes, AuthorizationCallback callback)
        {
            ArgumentNullException.ThrowIfNull(callback, nameof(callback));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(clientId, nameof(clientId));
            using var req = new HttpRequestMessage();
            req.Content = new FormUrlEncodedContent([
                new ("client_id", clientId),
                new ("scopes", string.Join(' ', scopes)),
                ]);
            req.RequestUri = new Uri("https://id.twitch.tv/oauth2/device");
            using var resp = await Client.SendAsync(req, CancellationToken);
            if (resp is null)
                return null;
            var dcm = JsonSerializer.Deserialize<DcfCodeMessage>(await resp.Content.ReadAsStringAsync());
            if (dcm is null)
                return null;
            
            using TcpListener listener = new();
            var auth_task = callback.Invoke(dcm.VerificationUri);
            if (auth_task is null)
                return null;
            var success = await auth_task;
        }

        #region IDisposable
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                TokenSource.Cancel();
                if (disposing)
                {
                    Client.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion //IDisposable
    }
    /// <summary>
    /// Callback for user to authorize the app
    /// </summary>
    /// <param name="url"></param>
    /// <returns>true if successful</returns>
    public delegate Task<bool> AuthorizationCallback(string url);
}
