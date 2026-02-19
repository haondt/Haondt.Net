using Haondt.Core.Models;

namespace Haondt.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static Optional<TValue> GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : notnull
        {
            if (dictionary.TryGetValue(key, out var value))
                return new(value);
            return new();
        }

        // named as such because Dictionary<TKey, TValue> can't differentiate between the extensions
        public static Optional<TValue> ReadOnlyGetValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) where TValue : notnull
        {
            if (dictionary.TryGetValue(key, out var value))
                return new(value);
            return new();
        }
    }
}

