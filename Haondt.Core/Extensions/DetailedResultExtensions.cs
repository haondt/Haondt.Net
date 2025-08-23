using Haondt.Core.Models;

namespace Haondt.Core.Extensions
{
    public static class DetailedResultExtensions
    {
        public static T3 Match<T1, T2, T3>(this DetailedResult<T1, T2> result, Func<T1, T3> success, Func<T2, T3> failure)
        {
            if (result.IsSuccessful)
                return success(result.Value);
            return failure(result.Reason);
        }

        public static T2 Match<T1, T2>(this DetailedResult<T1> result, Func<T2> success, Func<T1, T2> failure)
        {
            if (result.IsSuccessful)
                return success();
            return failure(result.Reason);
        }

        public static T2 Match<T1, T2>(this DetailedResult<T1> result, T2 success, Func<T1, T2> failure)
        {
            if (result.IsSuccessful)
                return success;
            return failure(result.Reason);
        }

        public static DetailedResult<T2, TReason> Map<T1, TReason, T2>(this DetailedResult<T1, TReason> result, Func<T1, T2> mapper)
        {
            if (result.IsSuccessful)
                return new(mapper(result.Value));
            return new(result.Reason);
        }

    }
}
