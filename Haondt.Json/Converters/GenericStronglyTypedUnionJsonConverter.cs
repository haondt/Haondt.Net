using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Haondt.Json.Converters
{
    public class GenericStronglyTypedUnionJsonConverter : JsonConverter
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
                var unionType = Nullable.GetUnderlyingType(objectType) ?? objectType;
                var surrogate = serializer.Deserialize<UnionSurrogate>(reader);
                if (surrogate == null)
                    return null;
                return surrogate.ToUnion(serializer);
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
                var surrogate = UnionSurrogate.FromUnion(value);
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
        public required List<string> UnionTypes { get; set; }
        public static UnionSurrogate FromUnion(object union)
        {
            var unionType = union.GetType();
            var genericTypes = unionType.GetGenericArguments();
            var methods = unionType.GetMethods();
            var unwrapMethod = methods.First(m => m.Name == "Unwrap");

            var value = unwrapMethod.Invoke(union, [])!;
            var valueTypeStrings = genericTypes.Select(t => StorageKeyConvert.ConvertStorageKeyPartType(t, null)).ToList();
            var valueTypeString = StorageKeyConvert.ConvertStorageKeyPartType(value.GetType(), null);
            return new()
            {
                Value = JToken.FromObject(value),
                UnionTypes = valueTypeStrings,
                ValueType = valueTypeString
            };
        }

        public object ToUnion(JsonSerializer serializer)
        {
            var valueType = StorageKeyConvert.ConvertStorageKeyPartType(ValueType, null);
            var unionTypes = UnionTypes.Select(t => StorageKeyConvert.ConvertStorageKeyPartType(t, null)).ToArray();
            var unionType = (UnionTypes.Count switch
            {
                2 => typeof(Union<,>),
                3 => typeof(Union<,,>),
                4 => typeof(Union<,,,>),
                5 => typeof(Union<,,,,>),
                6 => typeof(Union<,,,,,>),
                _ => throw new JsonSerializationException($"Unable to handle union with {UnionTypes.Count} types.")
            }).MakeGenericType(unionTypes);

            var constructor = unionType.GetConstructor([valueType]);
            return constructor!.Invoke([Value.ToObject(valueType, serializer)]);
        }

    }
}


