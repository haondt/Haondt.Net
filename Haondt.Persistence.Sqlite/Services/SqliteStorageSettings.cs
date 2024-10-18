namespace Haondt.Persistence.Sqlite.Services
{
    public class SqliteStorageSettings
    {
        public required string PrimaryTableName { get; set; }
        public required string ForeignKeyTableName { get; set; }
        public required string DatabasePath { get; set; }
        public bool StoreKeyStrings { get; set; } = false;
    }
}