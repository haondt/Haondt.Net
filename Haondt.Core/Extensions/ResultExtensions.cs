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

        public static T Match<T>(this Result result, Func<T> success, Func<T> failure) => result.IsSuccessful ? success() : failure();
        public static T Match<T>(this Result result, T success, Func<T> failure) => result.IsSuccessful ? success : failure();
        public static T Match<T>(this Result result, Func<T> success, T failure) => result.IsSuccessful ? success() : failure;
        public static T Match<T>(this Result result, T success, T failure) => result.IsSuccessful ? success : failure;

        public static T2 Match<T1, T2>(this Result<T1> result, Func<T1, T2> success, Func<T2> failure) => result.IsSuccessful ? success(result.Value) : failure();
        public static T2 Match<T1, T2>(this Result<T1> result, Func<T1, T2> success, T2 failure) => result.IsSuccessful ? success(result.Value) : failure;

        public static Result<T2> Map<T1, T2>(this Result<T1> result, Func<T1, T2> mapper) => result.IsSuccessful ? new(mapper(result.Value)) : new();

        public static T Expect<T>(this Result<T> result) => result.Value!;
        public static T Expect<T>(this Result<T> result, string errorMessage) => result.IsSuccessful ? result.Value : throw new InvalidOperationException(errorMessage);
        public static void Expect(this Result result)
        {
            if (!result.IsSuccessful)
                throw new InvalidOperationException("Result was not successful");
        }
        public static void Expect(this Result result, string errorMessage)
        {
            if (!result.IsSuccessful)
                throw new InvalidOperationException(errorMessage);
        }
    }
}
