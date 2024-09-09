using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haondt.Core.Models
{
    public readonly struct Result<T, TReason>
    {
        private readonly bool _success = true;
        private readonly T _value;
        private readonly TReason _reason;

        public Result(T value) { _value = value; _success = true; _reason = default!; }
        public Result(TReason reason) { _value = default!; _success = false; _reason = reason; }

        [MemberNotNullWhen(false, nameof(Reason))]
        public bool IsSuccessful => _success;

        public readonly T Value
        {
            get
            {
                if (_success)
                    return _value;
                throw new InvalidOperationException("Result was not successful");
            }
        }

        public readonly TReason? Reason
        {
            get
            {
                if (_success)
                    throw new InvalidOperationException("Result was successful");
                return _reason;
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
        private readonly bool _success = true;
        private readonly TReason _reason;

        [MemberNotNullWhen(false, nameof(Reason))]
        public readonly bool IsSuccessful => _success;
        public Result(TReason reason) { _reason = reason; _success = false; }

        public readonly TReason? Reason
        {
            get
            {
                if (_success)
                    throw new InvalidOperationException("Result was successful");
                return _reason;
            }
        }

        public Result<T, TReason> WithValue<T>(T value)
        {
            if (_success)
                return new Result<T, TReason>(value);
            return new Result<T, TReason>(_reason);
        }

        public Result<T, TReason> WithValue<T>()
        {
            if (_success)
                throw new ArgumentException($"result was successful, and requires a value to upgrade to {typeof(Result<T, TReason>)}");
            return new Result<T, TReason>(_reason);
        }

    }
}
