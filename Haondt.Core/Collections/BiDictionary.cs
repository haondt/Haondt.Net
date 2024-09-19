using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haondt.Core.Collections
{
    public class BiDictionary<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, TValue> _forward = new Dictionary<TKey, TValue>();
        private readonly Dictionary<TValue, TKey> _reverse = new Dictionary<TValue, TKey>();

        public static BiDictionary<TKey, TValue> FromForwardDictionary(Dictionary<TKey, TValue> forward)
        {
            var output = new BiDictionary<TKey, TValue>();
            foreach (var (key, value) in forward)
            {
                if (output._reverse.ContainsKey(value))
                    throw new ArgumentException("Duplicate value.");
                output._reverse[value] = key;
                output._forward[key] = value;
            }
            return output;
        }

        public static BiDictionary<TKey, TValue> FromReverseDictionary(Dictionary<TValue, TKey> reverse)
        {
            var output = new BiDictionary<TKey, TValue>();
            foreach (var (value, key) in reverse)
            {
                if (output._forward.ContainsKey(key))
                    throw new ArgumentException("Duplicate key.");
                output._forward[key] = value;
                output._reverse[value] = key;
            }
            return output;

        }

        // Add a pair to both dictionaries
        public void Add(TKey key, TValue value)
        {
            if (_forward.ContainsKey(key) || _reverse.ContainsKey(value))
                throw new ArgumentException("Duplicate key or value.");

            _forward[key] = value;
            _reverse[value] = key;
        }

        // Remove a pair from both dictionaries
        public bool RemoveByKey(TKey key)
        {
            if (_forward.TryGetValue(key, out var value))
            {
                _forward.Remove(key);
                _reverse.Remove(value);
                return true;
            }
            return false;
        }

        public bool RemoveByValue(TValue value)
        {
            if (_reverse.TryGetValue(value, out var key))
            {
                _reverse.Remove(value);
                _forward.Remove(key);
                return true;
            }
            return false;
        }

        // Lookup by key (string to Type)
        public TValue GetByKey(TKey key)
        {
            return _forward[key];
        }

        // Lookup by value (Type to string)
        public TKey GetByValue(TValue value)
        {
            return _reverse[value];
        }

        // Try to get value by key (with result checking)
        public bool TryGetByKey(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return _forward.TryGetValue(key, out value);
        }

        // Try to get key by value (with result checking)
        public bool TryGetByValue(TValue value, [MaybeNullWhen(false)] out TKey key)
        {
            return _reverse.TryGetValue(value, out key);
        }

        // Check if a key exists
        public bool ContainsKey(TKey key)
        {
            return _forward.ContainsKey(key);
        }

        // Check if a value exists
        public bool ContainsValue(TValue value)
        {
            return _reverse.ContainsKey(value);
        }
    }
}
