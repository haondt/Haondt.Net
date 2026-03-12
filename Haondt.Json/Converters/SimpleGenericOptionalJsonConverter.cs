using Haondt.Core.Models;
using Newtonsoft.Json;

namespace Haondt.Json.Converters
{
    /// <summary>
    /// A JSON converter for <see cref="Optional{T}"/> that serializes the inner value directly,
    /// without a wrapper object. An empty <see cref="Optional{T}"/> serializes as <c>null</c>,
    /// and a valued <see cref="Optional{T}"/> serializes as the inner value itself.
    /// <remarks>
    /// This converter does not support <see cref="Nullable{T}"/> of <see cref="Optional{T}"/>.
    /// If you need to distinguish between a null <see cref="Optional{T}"/>? and an empty
    /// <see cref="Optional{T}"/>, use <see cref="GenericOptionalJsonConverter"/> instead.
    /// </remarks>
    /// </summary>
    public class SimpleGenericOptionalJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            objectType.IsGenericType &&
            objectType.GetGenericTypeDefinition() == typeof(Optional<>);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var valueType = objectType.GetGenericArguments()[0];

            if (reader.TokenType == JsonToken.Null)
            {
                var empty = objectType.GetConstructor([])!;
                return empty.Invoke([]);
            }

            try
            {
                var value = serializer.Deserialize(reader, valueType);
                var constructor = objectType.GetConstructor([valueType])!;
                return constructor.Invoke([value]);
            }
            catch (Exception ex) when (ex is not JsonSerializationException)
            {
                throw new JsonSerializationException($"Failed to deserialize optional {objectType}", ex);
            }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var optionalType = value.GetType();
            var hasValueProperty = optionalType.GetProperty(nameof(Optional<>.HasValue))!;
            var valueProperty = optionalType.GetProperty(nameof(Optional<>.Value))!;

            if (!(bool)hasValueProperty.GetValue(value)!)
            {
                writer.WriteNull();
                return;
            }

            try
            {
                serializer.Serialize(writer, valueProperty.GetValue(value));
            }
            catch (Exception ex) when (ex is not JsonSerializationException)
            {
                throw new JsonSerializationException($"Failed to serialize optional", ex);
            }
        }
    }
}
