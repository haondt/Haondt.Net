using Haondt.Identity.StorageKey;
using Newtonsoft.Json;

namespace Haondt.Persistence.Converters
{
    public class GenericStorageKeyJsonConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(StorageKey<>))
                return true;
            return false;
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String
                || reader.Value is not string keyString
                || keyString == null)
                throw new JsonSerializationException("Unable to deserialize storage key: input string is null");

            return StorageKeyConvert.Deserialize(keyString).AsGeneric();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            if (value is not StorageKey storageKey)
                throw new JsonSerializationException($"Unexpected value when trying to serialize StorageKey. Expected StorageKey, got {value.GetType().FullName}");

            var serializedKey = StorageKeyConvert.Serialize(storageKey);
            writer.WriteValue(serializedKey);
        }
    }
}
