﻿using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Haondt.Json.Converters
{
    public class GenericOptionalJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType.IsGenericType
                && objectType.GetGenericTypeDefinition() == typeof(Optional<>))
                return true;
            if (Nullable.GetUnderlyingType(objectType) is Type underlyingType
                && underlyingType.IsGenericType
                && underlyingType.GetGenericTypeDefinition() == typeof(Optional<>))
                return true;
            return false;
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            try
            {

                var surrogate = serializer.Deserialize<OptionalSurrogate>(reader);
                if (surrogate == null)
                    return null;
                return surrogate.ToOptional(serializer);
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

            try
            {
                var surrogate = OptionalSurrogate.FromOptional(value);
                serializer.Serialize(writer, surrogate);
            }
            catch (Exception ex) when (ex is not JsonSerializationException)
            {
                throw new JsonSerializationException($"Failed to serialize optional", ex);
            }

        }
    }

    public class OptionalSurrogate
    {
        public JToken? Value { get; set; }
        public required string Type { get; set; }
        public static OptionalSurrogate FromOptional(object optional)
        {
            var optionalType = optional.GetType();
            var genericType = optionalType.GetGenericArguments()[0];
            var properties = optionalType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var hasValueProperty = properties.First(m => m.Name == "HasValue");
            var valueProperty = properties.First(m => m.Name == "Value");

            var hasValue = (bool)hasValueProperty.GetValue(optional)!;
            object? value = hasValue ? valueProperty.GetValue(optional) : null;

            var typeString = StorageKeyConvert.ConvertStorageKeyPartType(genericType, null);
            return new()
            {
                Value = value != null ? JToken.FromObject(value) : null,
                Type = typeString
            };
        }

        public object ToOptional(JsonSerializer serializer)
        {
            var valueType = StorageKeyConvert.ConvertStorageKeyPartType(Type, null);
            var optionalType = typeof(Optional<>).MakeGenericType(valueType);

            if (Value != null && Value.Type != JTokenType.Null)
            {
                var constructor = optionalType.GetConstructor([valueType]);
                return constructor!.Invoke([Value.ToObject(valueType)]);
            }
            else
            {
                var constructor = optionalType.GetConstructor([]);
                return constructor!.Invoke([]);
            }
        }

    }
}
