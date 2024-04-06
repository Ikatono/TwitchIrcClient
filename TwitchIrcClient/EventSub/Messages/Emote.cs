using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchIrcClient.EventSub.Messages
{
    public record class Emote
    {
        [JsonRequired]
        [JsonPropertyName("begin")]
        public int Begin { get; set; }
        [JsonRequired]
        [JsonPropertyName("end")]
        public int End { get; set; }
        [JsonRequired]
        [JsonPropertyName("id")]
        public string Id { get; set; }
        public Emote(int begin, int end, string id)
        {
            Begin = begin;
            End = end;
            Id = id;
        }
    }
}
