using Haondt.Core.Models;

namespace Haondt.Core.Extensions
{
    public static class ResultExtensions
    {
        public static Optional<T> AsOptional<T>(this Result<T> result) where T : notnull
        {
            return result.TryGetValue(out var value) ? new(value) : new();
        }

        public static Optional<T> AsOptional<T, TReason>(this DetailedResult<T, TReason> result) where T : notnull
        {
            return result.TryGetValue(out var value) ? new(value) : new();
        }
    }
}
