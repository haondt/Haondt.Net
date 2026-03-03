using Haondt.Core.Models;

namespace Haondt.Core.Converters
{
    public static class StringConverter
    {
        private delegate bool ParseMethod<TResult>(string value, out TResult result);
        private static InvalidCastException NewInvalidCastException<T>(string value) =>
            new InvalidCastException($"Cannot convert '{value}' to {typeof(T).FullName}");
        private static T Cast<T, TParsed>(TParsed parsed, string value)
        {
            if (parsed is T casted) return casted;
            throw NewInvalidCastException<T>(value);
        }
        private static T Convert<T, TIntermediate>(ParseMethod<TIntermediate> tryParse, string value)
        {
            if (!tryParse(value, out var parsed))
                throw NewInvalidCastException<T>(value);
            return Cast<T, TIntermediate>(parsed, value);
        }
        private static T ParseAbsoluteDateTime<T>(string value)
        {
            if (long.TryParse(value, out var ticks))
                return Cast<T, AbsoluteDateTime>(AbsoluteDateTime.Create(ticks), value);
            else if (DateTime.TryParse(value, out var dt))
                return Cast<T, AbsoluteDateTime>(AbsoluteDateTime.Create(dt), value);
            throw NewInvalidCastException<T>(value);
        }
        private static T FallBackTryParse<T>(Type targetType, string value)
        {
            if (targetType == typeof(Guid))
                return Convert<T, Guid>(Guid.TryParse, value);
            else if (targetType == typeof(AbsoluteDateTime))
                return ParseAbsoluteDateTime<T>(value);

            throw NewInvalidCastException<T>(value);
        }

        private static T ParseEnum<T>(Type targetType, string value)
        {
            if (Enum.TryParse(targetType, value, ignoreCase: true, out var enumValue))
                return Cast<T, object>(enumValue, value);
            throw NewInvalidCastException<T>(value);
        }
        public static T Parse<T>(string value)
        {
            var targetType = typeof(T);
            var underlying = Nullable.GetUnderlyingType(typeof(T));
            if (underlying != null)
            {
                if (value == string.Empty)
                    return default!;
                targetType = underlying;
            }

            if (targetType.IsEnum)
                return ParseEnum<T>(targetType, value);

            return Type.GetTypeCode(targetType) switch
            {
                TypeCode.SByte => Convert<T, sbyte>(sbyte.TryParse, value),
                TypeCode.Byte => Convert<T, byte>(byte.TryParse, value),
                TypeCode.Boolean => Convert<T, bool>(bool.TryParse, value),
                TypeCode.Char => Convert<T, char>(char.TryParse, value),
                TypeCode.String => (T)(object)value!,
                TypeCode.Int16 => Convert<T, short>(short.TryParse, value),
                TypeCode.Int32 => Convert<T, int>(int.TryParse, value),
                TypeCode.Int64 => Convert<T, long>(long.TryParse, value),
                TypeCode.UInt16 => Convert<T, ushort>(ushort.TryParse, value),
                TypeCode.UInt32 => Convert<T, uint>(uint.TryParse, value),
                TypeCode.UInt64 => Convert<T, ulong>(ulong.TryParse, value),
                TypeCode.Double => Convert<T, double>(double.TryParse, value),
                TypeCode.Single => Convert<T, float>(float.TryParse, value),
                TypeCode.Decimal => Convert<T, decimal>(decimal.TryParse, value),
                TypeCode.DateTime => Convert<T, DateTime>(DateTime.TryParse, value),
                _ => FallBackTryParse<T>(targetType, value)
            };

        }
    }
}
