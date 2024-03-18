using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLogger.IRC
{
    /// <summary>
    /// Holds key-value pairs of tags. Tag names are case-sensitive and DO NOT parse
    /// the "client prefix" or "vendor", instead treating these as part of the "key name".
    /// Because of this, repeat "key name" with different "client prefix" or "vendor" will
    /// be treated as distinct.
    /// </summary>
    public class MessageTags : IDictionary<string, string>
    {
        public Dictionary<string, string> Tags = [];
        public MessageTags()
        {

        }
        private enum ParseState
        {
            FindingKey,
            FindingValue,
            ValueEscaped,
        }
        //TODO this should be unit tested
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static MessageTags Parse(string s)
        {
            s.TrimStart('@');
            MessageTags tags = [];
            string key = "";
            string value = "";
            var state = ParseState.FindingKey;
            foreach (char c in s)
            {
                switch (state)
                {
                    case ParseState.FindingKey:
                        if (c == '=')
                            state = ParseState.FindingValue;
                        else if (c == ';')
                        {
                            state = ParseState.FindingKey;
                            tags.Add(key, "");
                            key = "";
                        }
                        else if (c == ' ')
                        {
                            tags.Add(key, "");
                            goto EndParse;
                        }
                        else
                            key += c;
                        break;
                    case ParseState.FindingValue:
                        if (c == '\\')
                        {
                            state = ParseState.ValueEscaped;
                        }
                        else if (c == ';')
                        {
                            tags.Add(key, value);
                            key = value = "";
                            state = ParseState.FindingKey;
                        }
                        else if (c == ' ')
                        {
                            tags.Add(key, value);
                            goto EndParse;
                        }
                        else if ("\r\n\0".Contains(c))
                            throw new ArgumentException("Invalid character in tag string", nameof(s));
                        else
                        {
                            value += c;
                        }
                        break;
                    case ParseState.ValueEscaped:
                        if (c == ':')
                        {
                            value += ';';
                            state = ParseState.FindingValue;
                        }
                        else if (c == 's')
                        {
                            value += ' ';
                            state = ParseState.FindingValue;
                        }
                        else if (c == '\\')
                        {
                            value += '\\';
                            state = ParseState.FindingValue;
                        }
                        else if (c == 'r')
                        {
                            value += '\r';
                            state = ParseState.FindingValue;
                        }
                        else if (c == 'n')
                        {
                            value += '\n';
                            state = ParseState.FindingValue;
                        }
                        else if (c == ';')
                        {
                            tags.Add(key, value);
                            key = value = "";
                            state = ParseState.FindingKey;
                        }
                        //spaces should already be stripped, but handle this as end of tags just in case
                        else if (c == ' ')
                        {
                            tags.Add(key, value);
                            key = value = "";
                            goto EndParse;
                        }
                        else if ("\r\n\0".Contains(c))
                            throw new ArgumentException("Invalid character in tag string", nameof(s));
                        else
                        {
                            value += c;
                            state = ParseState.FindingValue;
                        }
                        break;
                    default:
                        throw new InvalidEnumArgumentException("Invalid state enum");
                        
                }
            }
            //this is reached after processing the last character without hitting a space
            tags.Add(key, value);
        EndParse:
            return tags;
        }
        #region IDictionary<string, string?>
        public string this[string key] { get => ((IDictionary<string, string>)Tags)[key]; set => ((IDictionary<string, string>)Tags)[key] = value; }

        public ICollection<string> Keys => ((IDictionary<string, string>)Tags).Keys;

        public ICollection<string> Values => ((IDictionary<string, string>)Tags).Values;

        public int Count => ((ICollection<KeyValuePair<string, string>>)Tags).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, string>>)Tags).IsReadOnly;

        public void Add(string key, string value)
        {
            ((IDictionary<string, string>)Tags).Add(key, value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            ((ICollection<KeyValuePair<string, string>>)Tags).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<string, string>>)Tags).Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return ((ICollection<KeyValuePair<string, string>>)Tags).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, string>)Tags).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, string>>)Tags).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, string>>)Tags).GetEnumerator();
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, string>)Tags).Remove(key);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return ((ICollection<KeyValuePair<string, string>>)Tags).Remove(item);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        {
            return ((IDictionary<string, string>)Tags).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Tags).GetEnumerator();
        }
        #endregion //IDictionary<string, string?>

    }
}
