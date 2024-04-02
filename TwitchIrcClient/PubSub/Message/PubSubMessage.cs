using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TwitchIrcClient.PubSub.Message
{
    public class PubSubMessage : IDictionary<string, JsonNode?>
    {
        private readonly JsonObject Node;
        public string TypeString
        {
            get => (Node["type"] ?? throw new InvalidDataException()).ToJsonString();
            set
            {
                Node["type"] = value;
            }
        }
        //PING and PONG messages don't seem to have any data member
        public string? DataString =>
            Node["data"]?.ToJsonString();
        public string? Nonce
        {
            get => Node["nonce"]?.ToJsonString();
            set
            {
                Node["nonce"] = value;
            }
        }
        private PubSubMessage(JsonObject node)
        {
            Node = node;
        }
        public PubSubMessage() : this(new JsonObject())
        {

        }
        public string Serialize()
        {
            return Node.ToJsonString();
        }
        public static PubSubMessage Parse(string s)
        {
            var obj = JsonNode.Parse(s)
                      ?? throw new InvalidDataException();
            var psm = new PubSubMessage(obj as JsonObject
                ?? throw new InvalidOperationException());
            return psm;
        }
        public static PubSubMessage PING()
        {
            return new PubSubMessage
            {
                ["type"] = "PING",
            };
        }

        #region IDictionary<string, JsonNode?>
        public ICollection<string> Keys => ((IDictionary<string, JsonNode?>)Node).Keys;

        public ICollection<JsonNode?> Values => ((IDictionary<string, JsonNode?>)Node).Values;

        public int Count => Node.Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, JsonNode?>>)Node).IsReadOnly;

        public JsonNode? this[string key] { get => ((IDictionary<string, JsonNode?>)Node)[key]; set => ((IDictionary<string, JsonNode?>)Node)[key] = value; }

        

        public void Add(string key, JsonNode? value)
        {
            Node.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return Node.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return Node.Remove(key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out JsonNode? value)
        {
            return ((IDictionary<string, JsonNode?>)Node).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, JsonNode?> item)
        {
            Node.Add(item);
        }

        public void Clear()
        {
            Node.Clear();
        }

        public bool Contains(KeyValuePair<string, JsonNode?> item)
        {
            return ((ICollection<KeyValuePair<string, JsonNode?>>)Node).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, JsonNode?>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, JsonNode?>>)Node).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, JsonNode?> item)
        {
            return ((ICollection<KeyValuePair<string, JsonNode?>>)Node).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, JsonNode?>> GetEnumerator()
        {
            return Node.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Node).GetEnumerator();
        }
        #endregion //IDictionary<string, JsonNode?>
    }
}
