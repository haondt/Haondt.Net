using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace Haondt.Persistence.Tests
{
    public class MongoDbContainerFixture : IAsyncLifetime
    {
        private const string InitScript = """
            haondt = db.getSiblingDB('haondt');
            haondt.createCollection('haondt');

            haondt.haondt.createIndex({ "PrimaryKey": 1 }, { unique: true });
            haondt.haondt.createIndex({ "ForeignKeys": 1 });
            """;

        public MongoDbContainer Container { get; } = new MongoDbBuilder("mongo:5.0")
            .WithUsername("haondt")
            .WithPassword("haondt")
            .Build();

        public string ConnectionString { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await Container.StartAsync();
            ConnectionString = Container.GetConnectionString();
            await Container.ExecScriptAsync(InitScript);
        }

        public Task DisposeAsync() => Container.DisposeAsync().AsTask();
    }
}
