using Haondt.Identity.StorageKey;
using Haondt.Persistence.Postgresql.Services;
using Haondt.Persistence.Services;
using Microsoft.Extensions.Options;

namespace Haondt.Persistence.Tests
{
    public class PostgresqlStorageTests : AbstractStorageTests, IClassFixture<PostgresqlContainerFixture>
    {
        public PostgresqlStorageTests(PostgresqlContainerFixture fixture) : base(new TransientTransactionalBatchStorage(new PostgresqlStorage(Options.Create(new PostgresqlStorageSettings
        {
            Host = fixture.Host,
            Database = "haondt",
            Username = "haondt",
            Password = "haondt",
            ForeignKeyTableName = "foreignKeys",
            PrimaryTableName = "haondt",
            Port = fixture.Port,
            StoreKeyStrings = true
        }))))
        {
            StorageKeyConvert.DefaultSerializerSettings = new StorageKeySerializerSettings
            {
                TypeNameStrategy = TypeNameStrategy.SimpleTypeConverter,
                KeyEncodingStrategy = KeyEncodingStrategy.String
            };
        }

    }
}
