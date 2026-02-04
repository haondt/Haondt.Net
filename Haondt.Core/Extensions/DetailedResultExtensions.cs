using Haondt.Core.Models;

namespace Haondt.Core.Extensions
{
    public static class DetailedResultExtensions
    {
        extension<T, TReason>(DetailedResult<T, TReason> result) where T : notnull
        {
            public Optional<T> AsOptional()
            {
                return result.TryGetValue(out var value) ? new(value) : new();
            }
        }

        extension<T, TReason>(DetailedResult<T, TReason> result)
        {
            public T3 Match<T3>(Func<T, T3> success, Func<TReason, T3> failure)
            {
                if (result.IsSuccessful)
                    return success(result.Value);
                return failure(result.Reason);
            }

            public DetailedResult<T2, TReason> Map<T2>(Func<T, T2> mapper)
            {
                if (result.IsSuccessful)
                    return new(mapper(result.Value));
                return new(result.Reason);
            }
        }

        extension<T>(DetailedResult<T> result)
        {
            public T2 Match<T2>(Func<T2> success, Func<T, T2> failure)
            {
                if (result.IsSuccessful)
                    return success();
                return failure(result.Reason);
            }

            public T2 Match<T2>(T2 success, Func<T, T2> failure)
            {
                if (result.IsSuccessful)
                    return success;
                return failure(result.Reason);
            }
        }

    }
}
