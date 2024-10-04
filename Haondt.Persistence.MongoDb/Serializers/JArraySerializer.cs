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
    public class JArraySerializer : SerializerBase<JArray>
    {
        public override JArray Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonArray = BsonArraySerializer.Instance.Deserialize(context);
            var json = bsonArray.ToJson();
            return JArray.Parse(json);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, JArray value)
        {
            if (value == null)
            {
                context.Writer.WriteNull();
                return;
            }

            var array = BsonArray.Create(value.Select(x => x.ToString(Formatting.None)));
            BsonArraySerializer.Instance.Serialize(context, args, array);
        }
    }

}
