using Haondt.Persistence.Services;
using Microsoft.Data.Sqlite;

namespace Haondt.Persistence.Sqlite.Services
{
    public class CustomSqliteStorageOperation : StorageOperation
    {
        public required Action<SqliteConnection, SqliteTransaction> Execute { get; set; }
    }
}
