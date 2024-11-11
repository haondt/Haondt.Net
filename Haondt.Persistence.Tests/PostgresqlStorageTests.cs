using Haondt.Identity.StorageKey;
using Haondt.Persistence.Postgresql.Services;
using Haondt.Persistence.Services;
using Microsoft.Extensions.Options;

namespace Haondt.Persistence.Tests
{
    public class PostgresqlStorageTests : AbstractStorageTests
    {
        public PostgresqlStorageTests() : base(new TransientTransactionalBatchStorage(new PostgresqlStorage(Options.Create(new PostgresqlStorageSettings
        {
            Host = "localhost",
            Database = "haondt",
            Username = "haondt",
            Password = "haondt",
            ForeignKeyTableName = "foreignKeys",
            PrimaryTableName = "haondt",
            Port = 3432,
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
