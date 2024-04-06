using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchIrcClient.Authentication
{
    public record class DcfCodeMessage
    {
        [JsonRequired]
        [JsonPropertyName("device_code")]
        public string DeviceCode { get; set; }
        [JsonRequired]
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonRequired]
        [JsonPropertyName("interval")]
        public int Interval { get; set; }
        [JsonRequired]
        [JsonPropertyName("user_code")]
        public string UserCode { get; set; }
        [JsonRequired]
        [JsonPropertyName("verification_uri")]
        public string VerificationUri { get; set; }
    }
}
