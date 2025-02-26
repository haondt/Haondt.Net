using System.Diagnostics.CodeAnalysis;

namespace Haondt.Core.Models
{
    public readonly record struct DetailedResult<T, TReason>
    {
        private readonly bool _success;
        private readonly T _value;
        private readonly TReason _reason;

        public DetailedResult(T value) { _value = value; _success = true; _reason = default!; }
        public DetailedResult() { _value = default!; _success = true; _reason = default!; }
        public DetailedResult(TReason reason) { _value = default!; _success = false; _reason = reason; }

        public static DetailedResult<T, TReason> Fail(TReason reason)
        {
            return new DetailedResult<T, TReason>(reason);
        }

        public static DetailedResult<T, TReason> Succeed(T value)
        {
            return new DetailedResult<T, TReason>(value);
        }

        [MemberNotNullWhen(false, nameof(Reason))]
        [MemberNotNullWhen(true, nameof(Value))]
        public bool IsSuccessful => _success;

        public readonly T? Value
        {
            get
            {
                if (_success)
                    return _value;
                throw new InvalidOperationException("DetailedResult was not successful");
            }
        }

        public readonly TReason? Reason
        {
            get
            {
                if (_success)
                    throw new InvalidOperationException("DetailedResult was successful");
                return _reason;
            }
        }

        public static implicit operator DetailedResult<TReason>(DetailedResult<T, TReason> result)
        {
            if (result.IsSuccessful)
                return new DetailedResult<TReason>();
            return new DetailedResult<TReason>(result.Reason);
        }

        public static implicit operator Result<T>(DetailedResult<T, TReason> result) => result.IsSuccessful ? Result<T>.Success(result.Value) : Result<T>.Failure;
        public static implicit operator Result(DetailedResult<T, TReason> result) => result.IsSuccessful ? Result.Success : Result.Failure;
        public bool TryGetReason([NotNullWhen(true)] out TReason? reason)
        {
            reason = _reason;
            return !_success;
        }
        public bool TryGetValue([NotNullWhen(true)] out T? value)
        {
            value = _value;
            return _success;
        }
    }

    public readonly record struct DetailedResult<TReason>
    {
        private readonly bool _success;
        private readonly TReason _reason;

        [MemberNotNullWhen(false, nameof(Reason))]
        public readonly bool IsSuccessful => _success;

        public readonly TReason? Reason
        {
            get
            {
                if (_success)
                    return _reason;
                throw new InvalidOperationException("DetailedResult was successful");
            }
        }

        public DetailedResult(TReason reason) { _reason = reason; _success = false; }
        public DetailedResult() { _reason = default!; _success = true; }

        public static DetailedResult<TReason> Failure(TReason reason) => new DetailedResult<TReason>(reason);
        public static DetailedResult<TReason> Success { get; } = new DetailedResult<TReason>();

        public static implicit operator DetailedResult<TReason>(TReason value)
        {
            return new(value);
        }

        public static implicit operator Result(DetailedResult<TReason> result) => result.IsSuccessful ? Result.Success : Result.Failure;
        public bool TryGetReason([NotNullWhen(true)] out TReason? reason)
        {
            reason = _reason;
            return !_success;
        }
    }
}
