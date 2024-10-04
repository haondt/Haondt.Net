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
    public class JTokenSerializer : SerializerBase<JToken>
    {
        public override JToken Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            switch (context.Reader.CurrentBsonType)
            {
                case BsonType.Document:
                    return BsonSerializer.LookupSerializer<JObject>().Deserialize(context);
                case BsonType.Array:
                    return BsonSerializer.LookupSerializer<JArray>().Deserialize(context);
                default:
                    return BsonSerializer.LookupSerializer<JValue>().Deserialize(context);
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, JToken value)
        {
            if (value == null)
            {
                context.Writer.WriteNull();
                return;
            }

            switch (value.Type)
            {
                case JTokenType.Object:
                    BsonSerializer.LookupSerializer<JObject>().Serialize(context, args, (JObject)value);
                    break;
                case JTokenType.Array:
                    BsonSerializer.LookupSerializer<JArray>().Serialize(context, args, (JArray)value);
                    break;
                default:
                    BsonSerializer.LookupSerializer<JValue>().Serialize(context, args, (JValue)value);
                    break;
            }
        }
    }

}

