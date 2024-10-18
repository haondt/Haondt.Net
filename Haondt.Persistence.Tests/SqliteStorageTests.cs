using Haondt.Identity.StorageKey;
using Haondt.Persistence.Sqlite.Services;
using Microsoft.Extensions.Options;

namespace Haondt.Persistence.Tests
{
    public class SqliteStorageTests : AbstractStorageTests
    {
        public SqliteStorageTests() : base(new SqliteStorage(Options.Create(new SqliteStorageSettings
        {
            DatabasePath = "./testing.db",
            StoreKeyStrings = true,
            ForeignKeyTableName = "foreignKeys",
            PrimaryTableName = "haondt"
        })))
        {
            StorageKeyConvert.DefaultSerializerSettings = new StorageKeySerializerSettings
            {
                TypeNameStrategy = TypeNameStrategy.SimpleTypeConverter,
                KeyEncodingStrategy = KeyEncodingStrategy.String
            };
        }

    }
}
