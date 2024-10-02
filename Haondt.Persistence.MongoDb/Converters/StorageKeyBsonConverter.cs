using Haondt.Identity.StorageKey;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Haondt.Persistence.MongoDb.Converters
{
    public class StorageKeyBsonConverter : SerializerBase<StorageKey>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, StorageKey value)
            => context.Writer.WriteString(StorageKeyConvert.Serialize(value));

        public override StorageKey Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            => StorageKeyConvert.Deserialize(context.Reader.ReadString());
    }

    public class StorageKeyBsonConverter<T> : SerializerBase<StorageKey<T>>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, StorageKey<T> value)
            => context.Writer.WriteString(StorageKeyConvert.Serialize(value));

        public override StorageKey<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            => StorageKeyConvert.Deserialize<T>(context.Reader.ReadString());
    }
}
