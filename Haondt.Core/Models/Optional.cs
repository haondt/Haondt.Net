namespace Haondt.Core.Models
{
    public readonly struct Optional<T> where T : notnull
    {
        private readonly T _value;
        private readonly bool _hasValue;

        public static implicit operator Optional<T>(T value)
        {
            return new(value);
        }

        public Optional(T value)
        {
            _value = value;
            _hasValue = true;
        }

        public Optional()
        {
            _value = default!;
            _hasValue = false;
        }

        public bool HasValue => _hasValue;

        public T Value
        {
            get
            {
                if (!_hasValue)
                    throw new InvalidOperationException("No value present.");
                return _value;
            }

        }

    }
}
