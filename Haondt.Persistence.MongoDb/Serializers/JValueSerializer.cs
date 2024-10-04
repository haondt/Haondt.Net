using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haondt.Persistence.MongoDb.Serializers
{
    public class JValueSerializer : SerializerBase<JValue>
    {
        public override JValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            switch (context.Reader.CurrentBsonType)
            {
                case BsonType.Null:
                    context.Reader.ReadNull();
                    return JValue.CreateNull();
                case BsonType.String:
                    return new JValue(context.Reader.ReadString());
                case BsonType.Boolean:
                    return new JValue(context.Reader.ReadBoolean());
                case BsonType.Int32:
                    return new JValue(context.Reader.ReadInt32());
                case BsonType.Int64:
                    return new JValue(context.Reader.ReadInt64());
                case BsonType.Double:
                    return new JValue(context.Reader.ReadDouble());
                case BsonType.DateTime:
                    return new JValue(context.Reader.ReadDateTime());
                default:
                    throw new NotSupportedException($"Cannot deserialize BsonType {context.Reader.CurrentBsonType} to JValue");
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, JValue value)
        {
            if (value == null)
            {
                context.Writer.WriteNull();
                return;
            }

            switch (value.Type)
            {
                case JTokenType.Null:
                    context.Writer.WriteNull();
                    break;
                case JTokenType.String:
                    context.Writer.WriteString(value.Value<string>());
                    break;
                case JTokenType.Boolean:
                    context.Writer.WriteBoolean(value.Value<bool>());
                    break;
                case JTokenType.Integer:
                    context.Writer.WriteInt64(value.Value<long>());
                    break;
                case JTokenType.Float:
                    context.Writer.WriteDouble(value.Value<double>());
                    break;
                case JTokenType.Date:
                    context.Writer.WriteDateTime(value.Value<DateTime>().Ticks);
                    break;
                default:
                    throw new NotSupportedException($"Cannot serialize JValue of type {value.Type}");
            }
        }
    }
}
