namespace Haondt.Core.Converters
{
    public static class TypeConverter
    {
        public static T Coerce<T>(object value) where T : notnull
        {
            if (value is T casted)
                return casted;
            throw new InvalidCastException($"Cannot coerce object of type {value?.GetType()} to type {typeof(T)}");
        }


    }
}
