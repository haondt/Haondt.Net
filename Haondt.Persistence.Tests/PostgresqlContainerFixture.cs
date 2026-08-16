using Npgsql;
using Testcontainers.PostgreSql;

namespace Haondt.Persistence.Tests
{
    public class PostgresqlContainerFixture : IAsyncLifetime
    {
        private const string InitSql = """
            CREATE TABLE IF NOT EXISTS "haondt" (
                PrimaryKey TEXT PRIMARY KEY,
                KeyString TEXT NOT NULL,
                Value JSONB NOT NULL
            );

            CREATE TABLE IF NOT EXISTS "foreignKeys" (
                id SERIAL PRIMARY KEY,
                ForeignKey TEXT,
                KeyString TEXT NOT NULL,
                PrimaryKey TEXT REFERENCES "haondt"(PrimaryKey) ON DELETE CASCADE,
                CONSTRAINT unique_ForeignKey_PrimaryKey UNIQUE (ForeignKey, PrimaryKey)
            );

            CREATE INDEX IF NOT EXISTS idx_foreign_key
            ON "foreignKeys"(ForeignKey);
            """;

        public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("haondt")
            .WithUsername("haondt")
            .WithPassword("haondt")
            .Build();

        public string Host => Container.Hostname;
        public int Port => Container.GetMappedPublicPort(5432);

        public async Task InitializeAsync()
        {
            await Container.StartAsync();

            await using var connection = new NpgsqlConnection(Container.GetConnectionString());
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(InitSql, connection);
            await command.ExecuteNonQueryAsync();
        }

        public Task DisposeAsync() => Container.DisposeAsync().AsTask();
    }
}
