using Haondt.Core.Converters;
using Haondt.Core.Models;
using Haondt.Web.Core.Http;
using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;

namespace Haondt.Web.Core.Extensions
{
    public static class HttpRequestExtensions
    {
        public static IRequestData AsRequestData(this HttpRequest request)
        {
            return new TransientRequestData(
                () => request.Form,
                () => request.Query,
                () => request.Cookies,
                () => request.Headers);
        }

        public static T GetValue<T>(this IEnumerable<KeyValuePair<string, StringValues>> values, string key) where T : notnull
        {
            var result = values.TryGetValue<T>(key);
            if (result.HasValue)
                return result.Value;
            throw new KeyNotFoundException(key);
        }
        public static Optional<T> TryGetValue<T>(this IEnumerable<KeyValuePair<string, StringValues>> values, string key) where T : notnull
        {
            var kvp = values
                .Cast<KeyValuePair<string, StringValues>?>()
                .FirstOrDefault(kvp => kvp?.Key.Equals(key, StringComparison.OrdinalIgnoreCase) ?? false, null);

            var stringValue = kvp?.Value.Where(s => s != null).LastOrDefault(s => s != null, null);
            if (stringValue == null)
                return new();

            return new(StringConverter.Parse<T>(stringValue));
        }

        public static bool TryGetValue<T>(this IEnumerable<KeyValuePair<string, StringValues>> values, string key, [MaybeNullWhen(false)] out T value) where T : notnull
        {
            var result = TryGetValue<T>(values, key);
            if (result.HasValue)
            {
                value = result.Value;
                return true;
            }
            value = default;
            return false;

        }

        public static T GetValueOrDefault<T>(this IEnumerable<KeyValuePair<string, StringValues>> values, string key, T defaultValue) where T : notnull
        {
            var result = values.TryGetValue<T>(key);
            if (result.HasValue)
                return result.Value;
            return defaultValue;
        }

        public static IEnumerable<T> GetValues<T>(this IEnumerable<KeyValuePair<string, StringValues>> values, string key)
        {
            var kvp = values
                .Cast<KeyValuePair<string, StringValues>?>()
                .FirstOrDefault(kvp => kvp?.Key.Equals(key, StringComparison.OrdinalIgnoreCase) ?? false, null);

            var stringValues = kvp?.Value.Where(s => s != null).Cast<string>();
            if (stringValues == null)
                return [];

            return stringValues.Select(StringConverter.Parse<T>);
        }
    }
}
