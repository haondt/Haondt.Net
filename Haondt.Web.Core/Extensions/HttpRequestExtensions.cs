using Haondt.Core.Models;
using Haondt.Web.Core.Http;
using Microsoft.Extensions.Primitives;

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

        private delegate bool ParseMethod<TResult>(string? value, out TResult result);
        private static T ConvertValue<T>(string? value)
        {
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);


            T TryParse<TParser>(ParseMethod<TParser> parseMethod)
            {
                if (!parseMethod(value, out TParser parsedValue))
                    throw new InvalidCastException($"Cannot convert {value} to {typeof(T).FullName}");
                if (parsedValue is not T castedValue)
                    throw new InvalidCastException($"Cannot convert {value} to {typeof(T).FullName}");
                return castedValue;
            }

            T FallBackTryParse()
            {
                if (targetType == typeof(Guid))
                    return TryParse<Guid>(Guid.TryParse);
                throw new InvalidCastException($"Cannot convert {value} to {typeof(T).FullName}");
            }

            return Type.GetTypeCode(targetType) switch
            {
                TypeCode.Boolean => TryParse<bool>(bool.TryParse),
                TypeCode.String => (T)(object)value!,
                TypeCode.Int16 => TryParse<int>(int.TryParse),
                TypeCode.Int32 => TryParse<int>(int.TryParse),
                TypeCode.Int64 => TryParse<int>(int.TryParse),
                TypeCode.UInt16 => TryParse<int>(int.TryParse),
                TypeCode.UInt32 => TryParse<int>(int.TryParse),
                TypeCode.UInt64 => TryParse<int>(int.TryParse),
                TypeCode.Double => TryParse<double>(double.TryParse),
                TypeCode.Decimal => TryParse<decimal>(decimal.TryParse),
                TypeCode.DateTime => TryParse<DateTime>(DateTime.TryParse),
                _ => FallBackTryParse()
            };

        }

        public static T GetValue<T>(this IEnumerable<KeyValuePair<string, StringValues>> values, string key)
        {
            var result = values.TryGetValue<T>(key);
            if (result.HasValue)
                return result.Value;
            throw new KeyNotFoundException(key);
        }
        public static Optional<T> TryGetValue<T>(this IEnumerable<KeyValuePair<string, StringValues>> values, string key)
        {
            var kvp = values
                .Cast<KeyValuePair<string, StringValues>?>()
                .FirstOrDefault(kvp => kvp?.Key.Equals(key, StringComparison.OrdinalIgnoreCase) ?? false, null);

            var stringValue = kvp?.Value.Where(s => !string.IsNullOrEmpty(s)).LastOrDefault(s => !string.IsNullOrEmpty(s), null);
            if (stringValue == null)
                return new();

            return new(ConvertValue<T>(stringValue!));
        }

        public static T GetValueOrDefault<T>(this IEnumerable<KeyValuePair<string, StringValues>> values, string key, T defaultValue)
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

            var stringValues = kvp?.Value.Where(s => !string.IsNullOrEmpty(s)).Where(s => !string.IsNullOrEmpty(s));
            if (stringValues == null)
                return [];

            return stringValues.Select(v => ConvertValue<T>(v));
        }
    }
}
