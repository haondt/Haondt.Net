using Haondt.Core.Models;
using Newtonsoft.Json;

namespace Haondt.Json.Converters
{
    public class GenericUnionJsonConverter : JsonConverter
    {
        public static HashSet<Type> ConvertibleTypes = new()
        {
            typeof(Union<,>),
            typeof(Union<,,>),
            typeof(Union<,,,>),
            typeof(Union<,,,,>),
            typeof(Union<,,,,,>)
        };

        public override bool CanConvert(Type objectType)
        {
            // Handle Union<...> and Union<...>?
            if (objectType.IsGenericType && ConvertibleTypes.Contains(objectType.GetGenericTypeDefinition()))
                return true;

            if (Nullable.GetUnderlyingType(objectType) is Type underlyingType
                && underlyingType.IsGenericType
                && ConvertibleTypes.Contains(underlyingType.GetGenericTypeDefinition()))
                return true;

            return false;
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var unionType = Nullable.GetUnderlyingType(objectType) ?? objectType;

            var genericArgs = unionType.GetGenericArguments();
            foreach (var type in genericArgs)
            {
                try
                {
                    if (NarrowCheckSkipType(reader.TokenType, type))
                        continue;

                    var value = serializer.Deserialize(reader, type);
                    if (value != null)
                        return Activator.CreateInstance(unionType, value);
                }
                catch (JsonSerializationException)
                {
                    reader = new JsonTextReader(new StringReader(reader.Path));
                }
            }

            throw new JsonSerializationException($"Cannot deserialize value as {string.Join(" or ", genericArgs.Select(a => a.ToString()))}");
        }

        private HashSet<Type> NumericTypes = new()
        {
            typeof(byte),
            typeof(ushort),
            typeof(short),
            typeof(uint),
            typeof(int),
            typeof(long),
            typeof(ulong),
            typeof(double),
            typeof(float),
            typeof(decimal)
        };

        private bool NarrowCheckSkipType(JsonToken tokenType, Type targetType)
        {
            if (NumericTypes.Contains(targetType))
                return (tokenType != JsonToken.Integer && tokenType != JsonToken.Float);
            if (targetType == typeof(string))
                return tokenType != JsonToken.String;
            if (targetType == typeof(bool))
                return tokenType != JsonToken.Boolean;
            return false;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var unwrappedValue = value.GetType().GetMethod("Unwrap")?.Invoke(value, null);

            if (unwrappedValue == null)
                throw new JsonSerializationException("Unable to serialize Union: no active value found.");

            serializer.Serialize(writer, unwrappedValue);
        }
    }
}


