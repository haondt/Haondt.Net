using Haondt.Core.Converters;
using System.Diagnostics.CodeAnalysis;

namespace Haondt.Identity.StorageKey
{

    public class StorageKey : IEquatable<StorageKey>, IComparable<StorageKey>
    {
        public IReadOnlyList<StorageKeyPart> Parts { get; }
        public Type Type => Parts[^1].Type;

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            foreach (var part in Parts)
                hashCode.Add(part.GetHashCode());
            return hashCode.ToHashCode();
        }

        public override bool Equals([NotNullWhen(true)] object? obj) => obj is StorageKey sko && Equals(sko);
        public bool Equals(StorageKey? other)
        {
            if (other is null)
                return false;
            return Parts.SequenceEqual(other.Parts);
        }

        public static bool operator ==(StorageKey left, StorageKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StorageKey left, StorageKey right)
        {
            return !(left == right);
        }


        protected StorageKey(IReadOnlyList<StorageKeyPart> parts)
        {
            if (parts.Count < 1)
                throw new ArgumentException("Cannot initialize typed storage key without at least one part.");
            Parts = parts;
        }
        public static StorageKey Create(IReadOnlyList<StorageKeyPart> parts) => new(parts);
        public static StorageKey Create(Type type, string value) => new([new(type, value)]);
        public static StorageKey Empty(Type type) => Create(type, "");
        public StorageKey Extend(Type type, string value) => new([.. Parts, new(type, value)]);
        public StorageKey Extend(Type type) => new([.. Parts, new(type, "")]);

        /// <summary>
        /// Human readable representation of storage key
        /// </summary>
        /// <remarks>
        /// Note: this should not be used for a unique or stable representation of the storage key.
        /// For that you should use <see cref="StorageKeyConvert.Serialize"/>
        /// </remarks>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{nameof(StorageKey)}: {string.Join(',', Parts.Select(p => p.ToString()))}";
        }

        public int CompareTo(StorageKey? other)
        {
            if (other is null) return 1; // A non-null instance is always greater than a null one.

            int minLength = Math.Min(Parts.Count, other.Parts.Count);

            for (int i = 0; i < minLength; i++)
            {
                int typeComparison = String.Compare(Parts[i].Type.FullName, other.Parts[i].Type.FullName, StringComparison.Ordinal);
                if (typeComparison != 0) return typeComparison;

                int valueComparison = String.Compare(Parts[i].Value, other.Parts[i].Value, StringComparison.Ordinal);
                if (valueComparison != 0) return valueComparison;
            }

            // If all compared parts are equal, the longer list is considered greater.
            return Parts.Count.CompareTo(other.Parts.Count);
        }

    }

    public readonly struct StorageKeyPart(Type type, string value)
    {
        public Type Type { get; } = type;
        public string Value { get; } = value;
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Type);
            hashCode.Add(Value);
            return hashCode.ToHashCode();
        }

        public override string ToString()
        {
            return $"{Type.Name}+{Value}";
        }
    }

    public class StorageKey<T> : StorageKey, IEquatable<StorageKey<T>>, IComparable<StorageKey<T>> where T : notnull
    {
        private StorageKey(IReadOnlyList<StorageKeyPart> parts) : base(parts)
        {
            if (parts[^1].Type != typeof(T))
                throw new ArgumentException("Last type in part collection must be same as generic type");
        }

        public static new StorageKey<T> Create(IReadOnlyList<StorageKeyPart> parts) => new(parts);
        public static StorageKey<T> Create(string value) => new([new(typeof(T), value)]);
        public static new StorageKey<T> Empty { get; } = Create("");
        public StorageKey<T2> Extend<T2>(string value) where T2 : notnull => new([.. Parts, new(typeof(T2), value)]);
        public StorageKey<T2> Extend<T2>() where T2 : notnull => new([.. Parts, new(typeof(T2), "")]);

        public bool Equals(StorageKey<T>? other) => ((StorageKey)this).Equals(other);

        public int CompareTo(StorageKey<T>? other)
        {
            if (other is null) return 1; // A non-null instance is always greater than a null one.

            int minLength = Math.Min(Parts.Count, other.Parts.Count);

            for (int i = 0; i < minLength; i++)
            {
                int typeComparison = String.Compare(Parts[i].Type.FullName, other.Parts[i].Type.FullName, StringComparison.Ordinal);
                if (typeComparison != 0) return typeComparison;

                int valueComparison = String.Compare(Parts[i].Value, other.Parts[i].Value, StringComparison.Ordinal);
                if (valueComparison != 0) return valueComparison;
            }

            // If all compared parts are equal, the longer list is considered greater.
            return Parts.Count.CompareTo(other.Parts.Count);
        }
    }

    public static class StorageKeyPartExtensions
    {
        public static T Value<T>(this StorageKeyPart storageKeyPart) => StringConverter.Parse<T>(storageKeyPart.Value);
    }

    public static class StorageKeyExtensions
    {
        public static StorageKey<T> As<T>(this StorageKey storageKey) where T : notnull => StorageKey<T>.Create(storageKey.Parts);


        public static StorageKey AsGeneric(this StorageKey storageKey)
        {
            var storageKeyType = typeof(StorageKey<>).MakeGenericType(storageKey.Parts[^1].Type);
            var createMethod = storageKeyType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(m => m.Name == "Create")
                .First(m =>
                {
                    var p = m.GetParameters();
                    return p.Length == 1 && p[0].ParameterType == typeof(IReadOnlyList<StorageKeyPart>);
                });
            var genericStorageKey = createMethod.Invoke(null, [storageKey.Parts]);
            return (StorageKey)genericStorageKey!;
        }

        public static StorageKeyPart Last(this StorageKey storageKey) => storageKey.Parts[^1];
        public static string LastValue(this StorageKey storageKey) => storageKey.Last().Value;
        public static T LastValue<T>(this StorageKey storageKey) => storageKey.Last().Value<T>();
        public static Type LastType(this StorageKey storageKey) => storageKey.Last().Type;
        public static StorageKeyPart First(this StorageKey storageKey) => storageKey.Parts[0];
        public static string FirstValue(this StorageKey storageKey) => storageKey.First().Value;
        public static T FirstValue<T>(this StorageKey storageKey) => storageKey.First().Value<T>();
        public static Type FirstType(this StorageKey storageKey) => storageKey.First().Type;

        public static StorageKey SkipLast(this StorageKey storageKey)
            => StorageKey.Create(storageKey.Parts.Take(storageKey.Parts.Count - 1).ToList());
        public static StorageKey SkipLastValue(this StorageKey storageKey)
            => StorageKey.Create(storageKey.Parts.Take(storageKey.Parts.Count - 1)
                .Append(new StorageKeyPart(storageKey.Last().Type, "")).ToList());
        public static StorageKey SkipFirst(this StorageKey storageKey)
            => StorageKey.Create(storageKey.Parts.Skip(1).ToList());
        public static StorageKey SkipFirstValue(this StorageKey storageKey)
            => StorageKey.Create(storageKey.Parts.Skip(1)
                .Prepend(new StorageKeyPart(storageKey.First().Type, "")).ToList());
        public static StorageKey<T> SkipLast<T>(this StorageKey storageKey) where T : notnull
            => StorageKey<T>.Create(storageKey.Parts.Take(storageKey.Parts.Count - 1).ToList());
        public static StorageKey<T> SkipLastValue<T>(this StorageKey<T> storageKey) where T : notnull
            => StorageKey<T>.Create(storageKey.Parts.Take(storageKey.Parts.Count - 1)
                .Append(new StorageKeyPart(storageKey.Last().Type, "")).ToList());
        public static StorageKey<T> SkipFirst<T>(this StorageKey<T> storageKey) where T : notnull
            => StorageKey<T>.Create(storageKey.Parts.Skip(1).ToList());
        public static StorageKey<T> SkipFirstValue<T>(this StorageKey<T> storageKey) where T : notnull
            => StorageKey<T>.Create(storageKey.Parts.Skip(1)
                .Prepend(new StorageKeyPart(storageKey.First().Type, "")).ToList());

        public static StorageKeyPart Single(this StorageKey storageKey) => storageKey.Parts.Single();
        public static string SingleValue(this StorageKey storageKey) => storageKey.Parts.Single().Value;
        public static T SingleValue<T>(this StorageKey storageKey) where T : notnull => storageKey.Parts.Single().Value<T>();
    }
}
