using DotNext;
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
        private static Result<T> ConvertValue<T>(string? value)
        {
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);


            Result<T> TryParse<TParser>(ParseMethod<TParser> parseMethod)
            {
                if (!parseMethod(value, out TParser parsedValue))
                    return new(new InvalidCastException($"Cannot convert {value} to {typeof(T).FullName}"));
                if (parsedValue is not T castedValue)
                    return new(new InvalidCastException($"Cannot convert {value} to {typeof(T).FullName}"));
                return new(castedValue);
            }

            Result<T> FallBackTryParse()
            {
                if (targetType == typeof(Guid))
                    return TryParse<Guid>(Guid.TryParse);
                return new(new InvalidCastException($"Cannot convert {value} to {typeof(T).FullName}"));
            }

            return Type.GetTypeCode(targetType) switch
            {
                TypeCode.Boolean => TryParse<bool>(bool.TryParse),
                TypeCode.String => new Result<T>((T)(object)value!),
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

        public static Result<T> GetValue<T>(this IEnumerable<KeyValuePair<string, StringValues>> values, string key)
        {
            var uncastedValue = values.Single(kvp => kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Value.Last(s => !string.IsNullOrEmpty(s));
            return ConvertValue<T>(uncastedValue!);
        }

        public static T GetValueOrDefault<T>(this IEnumerable<KeyValuePair<string, StringValues>> values, string key, T defaultValue)
        {
            var result = GetValue<T>(values, key);
            if (result.IsSuccessful)
                return result.Value;
            return defaultValue;
        }

        public static Result<IEnumerable<T>> GetValues<T>(this IEnumerable<KeyValuePair<string, StringValues>> values, string key)
        {
            var kvp = values
                .Cast<KeyValuePair<string, StringValues>?>()
                .FirstOrDefault(kvp => kvp?.Key.Equals(key, StringComparison.OrdinalIgnoreCase) ?? false, null);

            var stringValues = kvp?.Value.Where(s => !string.IsNullOrEmpty(s)).Where(s => !string.IsNullOrEmpty(s));
            if (stringValues == null)
                return new([]);

            List<T> result = [];
            foreach (var stringValue in stringValues)
            {
                var conversionResult = ConvertValue<T>(stringValue);
                if (!conversionResult.IsSuccessful)
                    return new(conversionResult.Error);
                result.Append(conversionResult.Value);
            }
            return result;
        }
    }
}
