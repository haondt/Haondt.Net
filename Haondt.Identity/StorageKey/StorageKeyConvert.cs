using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Haondt.Identity.StorageKey
{
    public static class StorageKeyConvert
    {
        public static StorageKeySerializerSettings DefaultSerializerSettings { get; set; } = new();

        public static string Serialize(StorageKey storageKey, StorageKeySerializerSettings? settings = null)
        {
            settings ??= DefaultSerializerSettings;
            var parts = storageKey.Parts.Select(p =>
            {
                var typeString = ConvertStorageKeyPartType(p.Type, settings);

                string entry;
                switch (settings.KeyEncodingStrategy)
                {
                    case KeyEncodingStrategy.Base64:
                        var typeBytes = Encoding.UTF8.GetBytes(typeString);
                        var valueBytes = Encoding.UTF8.GetBytes(p.Value);
                        entry = $"{Convert.ToBase64String(typeBytes)}:{Convert.ToBase64String(valueBytes)}";
                        break;
                    case KeyEncodingStrategy.String:
                        var escapedTypeString = typeString.Replace(":", "::");
                        var escapedValueString = p.Value.Replace(":", "::");
                        entry = $"{escapedTypeString}{{:}}{escapedValueString}".Replace("+", "++");
                        break;
                    default:
                        throw new ArgumentException($"Unknown key encoding strategy {settings.KeyEncodingStrategy}");
                }
                return entry;
            });

            var seperator = settings.KeyEncodingStrategy switch
            {
                KeyEncodingStrategy.Base64 => ",",
                KeyEncodingStrategy.String => "{+}",
                _ => throw new ArgumentException($"Unknown key encoding strategy {settings.KeyEncodingStrategy}")
            };
            return string.Join(seperator, parts);
        }

        private static List<StorageKeyPart> DeserializeToParts(string data, StorageKeySerializerSettings? settings)
        {
            settings ??= DefaultSerializerSettings;
            var seperator = settings.KeyEncodingStrategy switch
            {
                KeyEncodingStrategy.Base64 => ",",
                KeyEncodingStrategy.String => "{+}",
                _ => throw new ArgumentException($"Unknown key encoding strategy {settings.KeyEncodingStrategy}")
            };

            return data.Split(seperator).Select(p =>
            {
                string typeString;
                string valueString;
                switch (settings.KeyEncodingStrategy)
                {
                    case KeyEncodingStrategy.Base64:
                        var sections = p.Split(':');
                        var valueBytes = Convert.FromBase64String(sections[1]);
                        var typeBytes = Convert.FromBase64String(sections[0]);
                        typeString = Encoding.UTF8.GetString(typeBytes);
                        valueString = Encoding.UTF8.GetString(valueBytes);
                        break;
                    case KeyEncodingStrategy.String:
                        sections = p.Replace("++", "+").Split("{:}");
                        typeString = sections[0].Replace("::", ":");
                        valueString = sections[1].Replace("::", ":");
                        break;
                    default:
                        throw new ArgumentException($"Unknown key encoding strategy {settings.KeyEncodingStrategy}");
                }
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