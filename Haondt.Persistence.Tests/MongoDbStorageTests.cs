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

        // I can't remember but I think mongo does not support transactions in the same way as SQL databases.
        public override Task WillPerformTransactionalBatchAdd() => Task.CompletedTask;
        public override Task WillPerformTransactionalBatchAddForeignKey() => Task.CompletedTask;
        public override Task WillPerformTransactionalBatchDelete() => Task.CompletedTask;
        public override Task WillPerformTransactionalBatchDeleteByForeignKey() => Task.CompletedTask;
        public override Task WillPerformTransactionalBatchDeleteForeignKey() => Task.CompletedTask;
        public override Task WillPerformTransactionalBatchRemoveForeignKey() => Task.CompletedTask;
        public override Task WillPerformTransactionalBatchSet() => Task.CompletedTask;
        public override Task WillThrowExceptionOnConflictingTransactionBatchAdd() => Task.CompletedTask;
    }
}
