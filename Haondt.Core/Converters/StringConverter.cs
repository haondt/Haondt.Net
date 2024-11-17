namespace Haondt.Core.Converters
{
    public static class StringConverter
    {
        private delegate bool ParseMethod<TResult>(string value, out TResult result);
        public static T Parse<T>(string value)
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
    }
}
