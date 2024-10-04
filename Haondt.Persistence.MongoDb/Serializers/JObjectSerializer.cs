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
    public class JObjectSerializer : SerializerBase<JObject>
    {
        public override JObject Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonDocument = BsonDocumentSerializer.Instance.Deserialize(context);
            var json = bsonDocument.ToJson();
            return JObject.Parse(json);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, JObject value)
        {
            if (value == null)
            {
                context.Writer.WriteNull();
                return;
            }

            var json = value.ToString(Formatting.None);
            var document = BsonDocument.Parse(json);
            BsonDocumentSerializer.Instance.Serialize(context, args, document);
        }
    }
}
