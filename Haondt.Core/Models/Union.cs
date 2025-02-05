using System.Diagnostics.CodeAnalysis;

namespace Haondt.Core.Models
{
    public readonly record struct Union<T1, T2>
        where T1 : notnull 
        where T2 : notnull 
    {
        private readonly object _value = default!;
        private readonly Type _type = default!;

        public Union(T1 value)
        {
            _value = value;
            _type = typeof(T1);
        }
        public Union(T2 value)
        {
            _value = value;
            _type = typeof(T2);
        }

        public object Unwrap()
        {
            return _value;
        }

        public bool Is<T>([MaybeNullWhen(false)] out T value) where T : notnull
        {
            if (typeof(T) == _type)
            {
                value = (T)_value;
                return true;
            }
            value = default;
            return false;
        }

        public Optional<T> As<T>() where T : notnull
        {
            if (Is<T>(out var value))
                return value;
            return new();
        }

        public T Cast<T>() where T : notnull
        {
            if (Is<T>(out var value))
                return value;
            throw new InvalidCastException($"Expected Union to contain type {typeof(T)}, but it was actually a {_type}.");
        }

        public static implicit operator Union<T1, T2>(T1 value) => new Union<T1, T2>(value);
        public static implicit operator Union<T1, T2>(T2 value) => new Union<T1, T2>(value);

        public static explicit operator T1(Union<T1, T2> union) => union.Cast<T1>();
        public static explicit operator T2(Union<T1, T2> union) => union.Cast<T2>();
    }
    public readonly record struct Union<T1, T2, T3>
        where T1 : notnull 
        where T2 : notnull 
        where T3 : notnull 
    {
        private readonly object _value = default!;
        private readonly Type _type = default!;

        public Union(T1 value)
        {
            _value = value;
            _type = typeof(T1);
        }
        public Union(T2 value)
        {
            _value = value;
            _type = typeof(T2);
        }
        public Union(T3 value)
        {
            _value = value;
            _type = typeof(T3);
        }

        public object Unwrap()
        {
            return _value;
        }

        public bool Is<T>([MaybeNullWhen(false)] out T value) where T : notnull
        {
            if (typeof(T) == _type)
            {
                value = (T)_value;
                return true;
            }
            value = default;
            return false;
        }

        public Optional<T> As<T>() where T : notnull
        {
            if (Is<T>(out var value))
                return value;
            return new();
        }

        public T Cast<T>() where T : notnull
        {
            if (Is<T>(out var value))
                return value;
            throw new InvalidCastException($"Expected Union to contain type {typeof(T)}, but it was actually a {_type}.");
        }

        public static implicit operator Union<T1, T2, T3>(T1 value) => new Union<T1, T2, T3>(value);
        public static implicit operator Union<T1, T2, T3>(T2 value) => new Union<T1, T2, T3>(value);
        public static implicit operator Union<T1, T2, T3>(T3 value) => new Union<T1, T2, T3>(value);

        public static explicit operator T1(Union<T1, T2, T3> union) => union.Cast<T1>();
        public static explicit operator T2(Union<T1, T2, T3> union) => union.Cast<T2>();
        public static explicit operator T3(Union<T1, T2, T3> union) => union.Cast<T3>();
    }
    public readonly record struct Union<T1, T2, T3, T4>
        where T1 : notnull 
        where T2 : notnull 
        where T3 : notnull 
        where T4 : notnull 
    {
        private readonly object _value = default!;
        private readonly Type _type = default!;

        public Union(T1 value)
        {
            _value = value;
            _type = typeof(T1);
        }
        public Union(T2 value)
        {
            _value = value;
            _type = typeof(T2);
        }
        public Union(T3 value)
        {
            _value = value;
            _type = typeof(T3);
        }
        public Union(T4 value)
        {
            _value = value;
            _type = typeof(T4);
        }

        public object Unwrap()
        {
            return _value;
        }

        public bool Is<T>([MaybeNullWhen(false)] out T value) where T : notnull
        {
            if (typeof(T) == _type)
            {
                value = (T)_value;
                return true;
            }
            value = default;
            return false;
        }

        public Optional<T> As<T>() where T : notnull
        {
            if (Is<T>(out var value))
                return value;
            return new();
        }

        public T Cast<T>() where T : notnull
        {
            if (Is<T>(out var value))
                return value;
            throw new InvalidCastException($"Expected Union to contain type {typeof(T)}, but it was actually a {_type}.");
        }

        public static implicit operator Union<T1, T2, T3, T4>(T1 value) => new Union<T1, T2, T3, T4>(value);
        public static implicit operator Union<T1, T2, T3, T4>(T2 value) => new Union<T1, T2, T3, T4>(value);
        public static implicit operator Union<T1, T2, T3, T4>(T3 value) => new Union<T1, T2, T3, T4>(value);
        public static implicit operator Union<T1, T2, T3, T4>(T4 value) => new Union<T1, T2, T3, T4>(value);

        public static explicit operator T1(Union<T1, T2, T3, T4> union) => union.Cast<T1>();
        public static explicit operator T2(Union<T1, T2, T3, T4> union) => union.Cast<T2>();
        public static explicit operator T3(Union<T1, T2, T3, T4> union) => union.Cast<T3>();
        public static explicit operator T4(Union<T1, T2, T3, T4> union) => union.Cast<T4>();
    }
    public readonly record struct Union<T1, T2, T3, T4, T5>
        where T1 : notnull 
        where T2 : notnull 
        where T3 : notnull 
        where T4 : notnull 
        where T5 : notnull 
    {
        private readonly object _value = default!;
        private readonly Type _type = default!;

        public Union(T1 value)
        {
            _value = value;
            _type = typeof(T1);
        }
        public Union(T2 value)
        {
            _value = value;
            _type = typeof(T2);
        }
        public Union(T3 value)
        {
            _value = value;
            _type = typeof(T3);
        }
        public Union(T4 value)
        {
            _value = value;
            _type = typeof(T4);
        }
        public Union(T5 value)
        {
            _value = value;
            _type = typeof(T5);
        }

        public object Unwrap()
        {
            return _value;
        }

        public bool Is<T>([MaybeNullWhen(false)] out T value) where T : notnull
        {
            if (typeof(T) == _type)
            {
                value = (T)_value;
                return true;
            }
            value = default;
            return false;
        }

        public Optional<T> As<T>() where T : notnull
        {
            if (Is<T>(out var value))
                return value;
            return new();
        }

        public T Cast<T>() where T : notnull
        {
            if (Is<T>(out var value))
                return value;
            throw new InvalidCastException($"Expected Union to contain type {typeof(T)}, but it was actually a {_type}.");
        }

        public static implicit operator Union<T1, T2, T3, T4, T5>(T1 value) => new Union<T1, T2, T3, T4, T5>(value);
        public static implicit operator Union<T1, T2, T3, T4, T5>(T2 value) => new Union<T1, T2, T3, T4, T5>(value);
        public static implicit operator Union<T1, T2, T3, T4, T5>(T3 value) => new Union<T1, T2, T3, T4, T5>(value);
        public static implicit operator Union<T1, T2, T3, T4, T5>(T4 value) => new Union<T1, T2, T3, T4, T5>(value);
        public static implicit operator Union<T1, T2, T3, T4, T5>(T5 value) => new Union<T1, T2, T3, T4, T5>(value);

        public static explicit operator T1(Union<T1, T2, T3, T4, T5> union) => union.Cast<T1>();
        public static explicit operator T2(Union<T1, T2, T3, T4, T5> union) => union.Cast<T2>();
        public static explicit operator T3(Union<T1, T2, T3, T4, T5> union) => union.Cast<T3>();
        public static explicit operator T4(Union<T1, T2, T3, T4, T5> union) => union.Cast<T4>();
        public static explicit operator T5(Union<T1, T2, T3, T4, T5> union) => union.Cast<T5>();
    }
    public readonly record struct Union<T1, T2, T3, T4, T5, T6>
        where T1 : notnull 
        where T2 : notnull 
        where T3 : notnull 
        where T4 : notnull 
        where T5 : notnull 
        where T6 : notnull 
    {
        private readonly object _value = default!;
        private readonly Type _type = default!;

        public Union(T1 value)
        {
            _value = value;
            _type = typeof(T1);
        }
        public Union(T2 value)
        {
            _value = value;
            _type = typeof(T2);
        }
        public Union(T3 value)
        {
            _value = value;
            _type = typeof(T3);
        }
        public Union(T4 value)
        {
            _value = value;
            _type = typeof(T4);
        }
        public Union(T5 value)
        {
            _value = value;
            _type = typeof(T5);
        }
        public Union(T6 value)
        {
            _value = value;
            _type = typeof(T6);
        }

        public object Unwrap()
        {
            return _value;
        }

        public bool Is<T>([MaybeNullWhen(false)] out T value) where T : notnull
        {
            if (typeof(T) == _type)
            {
                value = (T)_value;
                return true;
            }
            value = default;
            return false;
        }

        public Optional<T> As<T>() where T : notnull
        {
            if (Is<T>(out var value))
                return value;
            return new();
        }

        public T Cast<T>() where T : notnull
        {
            if (Is<T>(out var value))
                return value;
            throw new InvalidCastException($"Expected Union to contain type {typeof(T)}, but it was actually a {_type}.");
        }

        public static implicit operator Union<T1, T2, T3, T4, T5, T6>(T1 value) => new Union<T1, T2, T3, T4, T5, T6>(value);
        public static implicit operator Union<T1, T2, T3, T4, T5, T6>(T2 value) => new Union<T1, T2, T3, T4, T5, T6>(value);
        public static implicit operator Union<T1, T2, T3, T4, T5, T6>(T3 value) => new Union<T1, T2, T3, T4, T5, T6>(value);
        public static implicit operator Union<T1, T2, T3, T4, T5, T6>(T4 value) => new Union<T1, T2, T3, T4, T5, T6>(value);
        public static implicit operator Union<T1, T2, T3, T4, T5, T6>(T5 value) => new Union<T1, T2, T3, T4, T5, T6>(value);
        public static implicit operator Union<T1, T2, T3, T4, T5, T6>(T6 value) => new Union<T1, T2, T3, T4, T5, T6>(value);

        public static explicit operator T1(Union<T1, T2, T3, T4, T5, T6> union) => union.Cast<T1>();
        public static explicit operator T2(Union<T1, T2, T3, T4, T5, T6> union) => union.Cast<T2>();
        public static explicit operator T3(Union<T1, T2, T3, T4, T5, T6> union) => union.Cast<T3>();
        public static explicit operator T4(Union<T1, T2, T3, T4, T5, T6> union) => union.Cast<T4>();
        public static explicit operator T5(Union<T1, T2, T3, T4, T5, T6> union) => union.Cast<T5>();
        public static explicit operator T6(Union<T1, T2, T3, T4, T5, T6> union) => union.Cast<T6>();
    }
}
