using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Haondt.Identity.StorageKey
{
    public static class StorageKeyConvert
    {
        public static string Serialize(StorageKey storageKey)
        {
            var parts = storageKey.Parts.Select(p =>
            {
                var typeString = ConvertStorageKeyPartType(p.Type);
                var typeBytes = Encoding.UTF8.GetBytes(typeString);
                var valueBytes = Encoding.UTF8.GetBytes(p.Value);
                var entry = $"{Convert.ToBase64String(typeBytes)}:{Convert.ToBase64String(valueBytes)}";
                return entry;
            });
            return string.Join(',', parts);
        }

        private static List<StorageKeyPart> DeserializeToParts(string data)
        {
            return data.Split(',').Select(p =>
            {
                var sections = p.Split(':');
                var valueBytes = Convert.FromBase64String(sections[1]);
                var typeBytes = Convert.FromBase64String(sections[0]);
                var typeString = Encoding.UTF8.GetString(typeBytes);
                var valueString = Encoding.UTF8.GetString(valueBytes);
                return new StorageKeyPart(ConvertStorageKeyPartType(typeString), valueString);
            }).ToList();
        }

        public static StorageKey Deserialize(string data)
        {
            var parts = DeserializeToParts(data);
            return StorageKey.Create(parts);
        }

        public static StorageKey<T> Deserialize<T>(string data)
        {
            var parts = DeserializeToParts(data);
            return StorageKey<T>.Create(parts);
        }

        public static Type ConvertStorageKeyPartType(string storageKeyPartType)
        {
            return Type.GetType(storageKeyPartType)
                ?? throw new InvalidOperationException($"Unable to parse {nameof(Type)} from {nameof(storageKeyPartType)}");
        }

        public static string ConvertStorageKeyPartType(Type storageKeyPartType)
        {
            return storageKeyPartType.AssemblyQualifiedName
                ?? throw new InvalidOperationException($"Unable to retrieve {nameof(storageKeyPartType.AssemblyQualifiedName)}");
        }
    }
}