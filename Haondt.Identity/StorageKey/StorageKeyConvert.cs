using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Haondt.Identity.StorageKey
{
    public static class StorageKeyConvert
    {
        public static StorageKeySerializerSettings? DefaultSerializerSettings { get; set; }

        public static string Serialize(StorageKey storageKey, StorageKeySerializerSettings? settings = null)
        {
            var parts = storageKey.Parts.Select(p =>
            {
                var typeString = ConvertStorageKeyPartType(p.Type, settings);
                var typeBytes = Encoding.UTF8.GetBytes(typeString);
                var valueBytes = Encoding.UTF8.GetBytes(p.Value);
                var entry = $"{Convert.ToBase64String(typeBytes)}:{Convert.ToBase64String(valueBytes)}";
                return entry;
            });
            return string.Join(',', parts);
        }

        private static List<StorageKeyPart> DeserializeToParts(string data, StorageKeySerializerSettings? settings)
        {
            return data.Split(',').Select(p =>
            {
                var sections = p.Split(':');
                var valueBytes = Convert.FromBase64String(sections[1]);
                var typeBytes = Convert.FromBase64String(sections[0]);
                var typeString = Encoding.UTF8.GetString(typeBytes);
                var valueString = Encoding.UTF8.GetString(valueBytes);
                return new StorageKeyPart(ConvertStorageKeyPartType(typeString, settings), valueString);
            }).ToList();
        }

        public static StorageKey Deserialize(string data, StorageKeySerializerSettings? settings = null)
        {
            var parts = DeserializeToParts(data, settings);
            return StorageKey.Create(parts);
        }

        public static StorageKey<T> Deserialize<T>(string data, StorageKeySerializerSettings? settings = null)
        {
            var parts = DeserializeToParts(data, settings);
            return StorageKey<T>.Create(parts);
        }

        public static Type ConvertStorageKeyPartType(string storageKeyPartType, StorageKeySerializerSettings? settings)
        {
            settings ??= DefaultSerializerSettings;
            switch (settings?.TypeNameStrategy ?? TypeNameStrategy.AssemblyQualifiedName)
            {
                case TypeNameStrategy.AssemblyQualifiedName:
                case TypeNameStrategy.StringName:
                case TypeNameStrategy.FullName:
                    return Type.GetType(storageKeyPartType)
                        ?? throw new InvalidOperationException($"Unable to parse {nameof(Type)} from {nameof(storageKeyPartType)}");
                case TypeNameStrategy.LookupTable:
                    return settings!.LookupTable.Value.GetByValue(storageKeyPartType);
                case TypeNameStrategy.SimpleTypeConverter:
                    return SimpleTypeConverter.StringToType(storageKeyPartType);
            };
            throw new InvalidOperationException($"Unknown {nameof(TypeNameStrategy)}: {settings?.TypeNameStrategy}");
        }

        public static string ConvertStorageKeyPartType(Type storageKeyPartType, StorageKeySerializerSettings? settings)
        {
            settings ??= DefaultSerializerSettings;
            return (settings?.TypeNameStrategy ?? TypeNameStrategy.AssemblyQualifiedName) switch
            {
                TypeNameStrategy.AssemblyQualifiedName => storageKeyPartType.AssemblyQualifiedName
                    ?? throw new InvalidOperationException($"Unable to retrieve {nameof(storageKeyPartType.AssemblyQualifiedName)}"),
                TypeNameStrategy.StringName => storageKeyPartType.ToString(),
                TypeNameStrategy.Name => storageKeyPartType.Name,
                TypeNameStrategy.FullName => storageKeyPartType.FullName
                    ?? throw new InvalidOperationException($"Unable to retrieve {nameof(storageKeyPartType.FullName)}"),
                TypeNameStrategy.LookupTable => settings!.LookupTable.Value.GetByKey(storageKeyPartType),
                TypeNameStrategy.SimpleTypeConverter => SimpleTypeConverter.TypeToString(storageKeyPartType),
                _ => throw new InvalidOperationException($"Unknown {nameof(TypeNameStrategy)}: {settings?.TypeNameStrategy}")
            };
        }
    }
}