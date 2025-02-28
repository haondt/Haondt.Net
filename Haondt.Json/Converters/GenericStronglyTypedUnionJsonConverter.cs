using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Haondt.Json.Converters
{
    public class GenericStronglyTypedUnionJsonConverter(StorageKeySerializerSettings? typeConversionSerializerSettings = null) : JsonConverter
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

            try
            {
                var surrogate = serializer.Deserialize<UnionSurrogate>(reader);
                if (surrogate == null)
                    return null;
                var unionType = Nullable.GetUnderlyingType(objectType) ?? objectType;
                return surrogate.ToUnion(serializer, unionType, typeConversionSerializerSettings);
            }
            catch (Exception ex) when (ex is not JsonSerializationException)
            {
                throw new JsonSerializationException($"Failed to deserialize union {objectType}", ex);
            }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            try
            {
                var surrogate = UnionSurrogate.FromUnion(value, serializer, typeConversionSerializerSettings);
                serializer.Serialize(writer, surrogate);
            }
            catch (Exception ex) when (ex is not JsonSerializationException)
            {
                throw new JsonSerializationException($"Failed to serialize union", ex);

            }
        }
    }
    public class UnionSurrogate
    {
        public required JToken Value { get; set; }
        public required string ValueType { get; set; }
        public static UnionSurrogate FromUnion(object union, JsonSerializer serializer, StorageKeySerializerSettings? typeConversionSerializerSettings = null)
        {
            var unionType = union.GetType();
            var genericTypes = unionType.GetGenericArguments();
            var methods = unionType.GetMethods();
            var unwrapMethod = methods.First(m => m.Name == "Unwrap");

            var value = unwrapMethod.Invoke(union, [])!;
            var valueTypeString = StorageKeyConvert.ConvertStorageKeyPartType(value.GetType(), typeConversionSerializerSettings);
            return new()
            {
                Value = JToken.FromObject(value, serializer),
                ValueType = valueTypeString
            };
        }

        public object ToUnion(JsonSerializer serializer, Type unionType, StorageKeySerializerSettings? typeConversionSerializerSettings = null)
        {
            var valueType = StorageKeyConvert.ConvertStorageKeyPartType(ValueType, typeConversionSerializerSettings);
            var constructor = unionType.GetConstructor([valueType]);
            return constructor!.Invoke([Value.ToObject(valueType, serializer)]);
        }

    }
}


