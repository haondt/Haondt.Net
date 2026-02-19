namespace Haondt.Core.Models
{
    public readonly record struct NormalizedString(string Value)
    {
        public static NormalizedString Create(string value)
        {
            return new NormalizedString(value?.Trim().ToUpperInvariant() ?? string.Empty);
        }

        public static implicit operator string(NormalizedString normalizedString)
        {
            return normalizedString.Value;
        }

        public static implicit operator NormalizedString(string value)
        {
            return Create(value);
        }
    }
}

