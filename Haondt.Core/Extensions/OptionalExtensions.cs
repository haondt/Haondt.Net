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

        public static T? Unwrap<T>(this Optional<T> optional) where T : class
        {
            if (optional.TryGetValue(out var value))
                return value;
            return default;
        }

        public static Result<T> AsResult<T>(this Optional<T> optional) where T : notnull
        {
            return optional.TryGetValue(out var value) ? Result<T>.Success(value) : Result<T>.Failure;
        }
    }

    // hack to avoid method signature conflicts
    public static class OptionalExtensions2
    {
        public static T? Unwrap<T>(this Optional<T> optional) where T : struct
        {
            if (optional.TryGetValue(out var value))
                return value;
            return null;
        }
    }
}
