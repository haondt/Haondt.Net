using Haondt.Identity.StorageKey;
using Haondt.Persistence.MongoDb.Converters;
using Haondt.Persistence.MongoDb.Services;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Haondt.Persistence.Tests
{
    public class MongoDbStorageTests : AbstractStorageTests
    {
        static MongoDbStorageTests()
        {
            BsonSerializer.RegisterSerializer(new ObjectSerializer(type => true));
            BsonSerializer.RegisterGenericSerializerDefinition(typeof(StorageKey<>), typeof(StorageKeyBsonConverter<>));
            BsonSerializer.RegisterSerializer(typeof(StorageKey), new StorageKeyBsonConverter());
        }

        private static MongoClientSettings MongoClientSettings
        {
            get
            {

                var settings = MongoClientSettings.FromConnectionString("mongodb://haondt:haondt@localhost:37017/");
                settings.ClusterConfigurator = cb =>
                {
                    // for debugging
                    //cb.Subscribe<CommandStartedEvent>(e =>
                    //{
                    //    var x = e.CommandName;
                    //    var y = e.Command.ToJson();
                    //    ;
                    //});
                };
                return settings;
            }
        }

        public MongoDbStorageTests() : base(
            new MongoDbStorage(
                "haondt",
                "haondt",
                new MongoClient(MongoClientSettings)))
        {
            StorageKeyConvert.DefaultSerializerSettings = new StorageKeySerializerSettings
            {
                TypeNameStrategy = TypeNameStrategy.SimpleTypeConverter,
                KeyEncodingStrategy = KeyEncodingStrategy.String
            };
        }
    }
}
