using Haondt.Core.Models;

namespace Haondt.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static Optional<T> AsOptional<T>(this T? value) where T : struct
        {
            return value.HasValue ? new(value.Value) : new();
        }

        public static Optional<T> AsOptional<T>(this T? value) where T : notnull
        {
            return value is not null ? new(value) : new();
        }

    }
}
