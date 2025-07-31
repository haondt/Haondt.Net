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
        public static Optional<T> Or<T>(this Optional<T> optional, Optional<T> alternativeValue) where T : notnull
        {
            if (optional.HasValue)
                return optional.Value;
            return alternativeValue;
        }

        public static Optional<T> Or<T>(this Optional<T> optional, Func<Optional<T>> alternativeValueFactory) where T : notnull
        {
            if (optional.HasValue)
                return optional.Value;
            return alternativeValueFactory();
        }

        [Obsolete("Use Map<T1, T2>")]
        public static Optional<T2> As<T1, T2>(this Optional<T1> optional, Func<T1, T2> converter) where T1 : notnull where T2 : notnull
        {
            if (optional.HasValue)
                return converter(optional.Value);
            return new();
        }

        [Obsolete("Use Map<T1, T2>")]
        public static async Task<Optional<T2>> As<T1, T2>(this Optional<T1> optional, Func<T1, Task<T2>> converter) where T1 : notnull where T2 : notnull
        {
            if (optional.HasValue)
                return await converter(optional.Value);
            return new();
        }

        public static Optional<T2> Map<T1, T2>(this Optional<T1> optional, Func<T1, T2> mapper) where T1 : notnull where T2 : notnull
        {
            if (optional.HasValue)
                return mapper(optional.Value);
            return new();
        }

        public static async Task<Optional<T2>> Map<T1, T2>(this Optional<T1> optional, Func<T1, Task<T2>> mapper) where T1 : notnull where T2 : notnull
        {
            if (optional.HasValue)
                return await mapper(optional.Value);
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

        public static Optional<T2> Bind<T1, T2>(this Optional<T1> optional, Func<T1, Optional<T2>> projection) where T1 : notnull where T2 : notnull =>
            optional.TryGetValue(out var value) ? projection(value) : new();

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
