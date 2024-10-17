using System.Diagnostics.CodeAnalysis;

namespace Haondt.Core.Models
{
    public readonly struct Result<T, TReason>
    {
        private readonly bool _fail;
        private readonly T _value;
        private readonly TReason _reason;

        public Result(T value) { _value = value; _fail = false; _reason = default!; }
        public Result() { _value = default!; _fail = false; _reason = default!; }
        public Result(TReason reason) { _value = default!; _fail = true; _reason = reason; }

        public static Result<T, TReason> Fail(TReason reason)
        {
            return new Result<T, TReason>(reason);
        }

        public static Result<T, TReason> Succeed(T value)
        {
            return new Result<T, TReason>(value);
        }

        [MemberNotNullWhen(false, nameof(Reason))]
        public bool IsSuccessful => !_fail;

        public readonly T Value
        {
            get
            {
                if (_fail)
                    throw new InvalidOperationException("Result was not successful");
                return _value;
            }
        }

        public readonly TReason? Reason
        {
            get
            {
                if (_fail)
                    return _reason;
                throw new InvalidOperationException("Result was successful");
            }
        }

        public static implicit operator Result<TReason>(Result<T, TReason> result)
        {
            if (result.IsSuccessful)
                return new Result<TReason>();
            return new Result<TReason>(result.Reason);
        }

        public Result<TReason> WithoutValue()
        {
            return this;
        }
    }

    public readonly struct Result<TReason>
    {
        private readonly bool _fail;
        private readonly TReason _reason;

        [MemberNotNullWhen(false, nameof(Reason))]
        public readonly bool IsSuccessful => !_fail;
        public Result(TReason reason) { _reason = reason; _fail = true; }
        public Result() { _reason = default!; _fail = false; }

        public static Result<TReason> Fail(TReason reason)
        {
            return new Result<TReason>(reason);
        }

        public static Result<TReason> Succeed()
        {
            return new Result<TReason>();
        }

        public readonly TReason? Reason
        {
            get
            {
                if (_fail)
                    return _reason;
                throw new InvalidOperationException("Result was successful");
            }
        }

        public Result<T, TReason> WithValue<T>(T value)
        {
            if (!_fail)
                return new Result<T, TReason>(_reason);
            return new Result<T, TReason>(value);
        }

        public Result<T, TReason> WithValue<T>()
        {
            if (!_fail)
                return new Result<T, TReason>(_reason);
            throw new ArgumentException($"result was successful, and requires a value to upgrade to {typeof(Result<T, TReason>)}");
        }

    }
}
