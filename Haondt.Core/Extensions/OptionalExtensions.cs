using Haondt.Core.Models;

namespace Haondt.Core.Extensions
{
    public static class OptionalExtensions
    {
        public static T Or<T>(this Optional<T> optional, T defaultValue) where T : notnull
        {
            if (optional.HasValue)
                return optional.Value;
            return defaultValue;
        }
        public static T Or<T>(this Optional<T> optional, Func<T> defaultValueFactory) where T : notnull
        {
            if (optional.HasValue)
                return optional.Value;
            return defaultValueFactory();
        }

        public static Optional<T2> As<T1, T2>(this Optional<T1> optional, Func<T1, T2> converter) where T1 : notnull where T2 : notnull
        {
            if (optional.HasValue)
                return converter(optional.Value);
            return new();
        }
        public static async Task<Optional<T2>> As<T1, T2>(this Optional<T1> optional, Func<T1, Task<T2>> converter) where T1 : notnull where T2 : notnull
        {
            if (optional.HasValue)
                return await converter(optional.Value);
            return new();
        }

    }
}
