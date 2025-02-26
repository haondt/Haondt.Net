using System.Diagnostics.CodeAnalysis;

namespace Haondt.Core.Models
{
    public readonly record struct Result<T>
    {
        private readonly T _value;
        private readonly bool _success;

        [MemberNotNullWhen(true, nameof(Value))]
        public readonly bool IsSuccessful => _success;
        public Result(T value) { _value = value; _success = true; }
        public Result() { _value = default!; _success = false; }

        public static Result<T> Success(T value)
        {
            return new Result<T>(value);
        }

        public static Result<T> Failure { get; } = new Result<T>();

        public T? Value
        {
            get
            {
                if (_success)
                    return _value;
                throw new InvalidOperationException("Result was not successful");
            }
        }

        public static implicit operator Result<T>(T value)
        {
            return new(value);
        }

        public static implicit operator Result(Result<T> result) => result.IsSuccessful ? Result.Success : Result.Failure;

        public bool TryGetValue([NotNullWhen(true)] out T? value)
        {
            value = _value;
            return _success;
        }
    }

    public readonly record struct Result
    {
        private readonly bool _success;
        public readonly bool IsSuccessful => _success;
        public Result(bool isSuccessful) { _success = !isSuccessful; }

        public static Result Failure { get; } = new Result(false);
        public static Result Success { get; } = new Result(true);
    }
}
