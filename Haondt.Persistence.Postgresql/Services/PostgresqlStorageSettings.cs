namespace Haondt.Persistence.Postgresql.Services
{
    public class PostgresqlStorageSettings
    {
        public required string Host { get; set; }
        public required string Database { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public int Port { get; set; } = 5432;
        public required string PrimaryTableName { get; set; }
        public required string ForeignKeyTableName { get; set; }
        public bool StoreKeyStrings { get; set; } = false;
    }
}