using Haondt.Core.Models;

namespace Haondt.Core.Extensions
{
    public static class ResultExtensions
    {

        extension<T>(Result<T> result) where T : notnull
        {
            public Optional<T> AsOptional() => result.TryGetValue(out var value) ? new(value) : new();
        }


        extension<T>(Result<T> result)
        {
            public T Match(Func<T> success, Func<T> failure) => result.IsSuccessful ? success() : failure();
            public T Match(T success, Func<T> failure) => result.IsSuccessful ? success : failure();
            public T Match(Func<T> success, T failure) => result.IsSuccessful ? success() : failure;
            public T Match(T success, T failure) => result.IsSuccessful ? success : failure;

            public T2 Match<T2>(Func<T, T2> success, Func<T2> failure) => result.IsSuccessful ? success(result.Value) : failure();
            public T2 Match<T2>(Func<T, T2> success, T2 failure) => result.IsSuccessful ? success(result.Value) : failure;

            public Result<T2> Map<T2>(Func<T, T2> mapper) => result.IsSuccessful ? new(mapper(result.Value)) : new();

            public T Expect() => result.Value!;
            public T Expect(string errorMessage) => result.IsSuccessful ? result.Value : throw new InvalidOperationException(errorMessage);
        }

        extension(Result result)
        {
            public void Expect()
            {
                if (!result.IsSuccessful)
                    throw new InvalidOperationException("Result was not successful");
            }

            public void Expect(string errorMessage)
            {
                if (!result.IsSuccessful)
                    throw new InvalidOperationException(errorMessage);
            }
        }

    }
}
