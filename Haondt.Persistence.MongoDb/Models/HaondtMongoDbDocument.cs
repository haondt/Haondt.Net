using Haondt.Identity.StorageKey;
using MongoDB.Bson.Serialization.Attributes;

namespace Haondt.Persistence.MongoDb.Models
{
    [BsonIgnoreExtraElements]
    public class HaondtMongoDbDocument
    {
        public required StorageKey PrimaryKey { get; set; }
        public List<StorageKey> ForeignKeys { get; set; } = [];
        public required object? Value { get; set; }
    }
}
