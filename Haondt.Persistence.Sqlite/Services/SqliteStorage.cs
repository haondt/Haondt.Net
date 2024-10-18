using Haondt.Core.Converters;
using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.Converters;
using Haondt.Persistence.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Haondt.Persistence.Sqlite.Services
{
    public class SqliteStorage : IStorage
    {
        private readonly SqliteStorageSettings _settings;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly string _connectionString;
        private readonly string _primaryTableName;
        private readonly string _foreignKeyTableName;

        public SqliteStorage(IOptions<SqliteStorageSettings> options)
        {
            _settings = options.Value;
            _serializerSettings = new JsonSerializerSettings();
            _connectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = _settings.DatabasePath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Private
            }.ToString();
            ConfigureSerializerSettings(_serializerSettings);
            _primaryTableName = SanitizeTableName(_settings.PrimaryTableName);
            _foreignKeyTableName = SanitizeTableName(_settings.ForeignKeyTableName);
            InitializeDb();
        }

        private JsonSerializerSettings ConfigureSerializerSettings(JsonSerializerSettings settings)
        {
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            settings.Formatting = Formatting.None;
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Converters.Add(new GenericStorageKeyJsonConverter());
            return settings;
        }

        private string SanitizeTableName(string tableName)
        {
            // Escape any double quotes by replacing them with two double quotes
            var sanitized = tableName.Replace("\"", "\"\"");

            // Surround the sanitized table name with double quotes
            return $"\"{sanitized}\"";
        }

        private SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var walCommand = connection.CreateCommand();
            walCommand.CommandText = "PRAGMA journal_mode=WAL;";
            walCommand.ExecuteNonQuery();

            using var enableForeignKeysCommand = new SqliteCommand("PRAGMA foreign_keys = ON;", connection);
            enableForeignKeysCommand.ExecuteNonQuery();

            return connection;
        }

        private void WithConnection(Action<SqliteConnection> action)
        {
            using var connection = GetConnection();
            action(connection);
        }

        private T WithConnection<T>(Func<SqliteConnection, T> action)
        {
            using var connection = GetConnection();
            return action(connection);
        }

        private void WithTransaction(Action<SqliteConnection, SqliteTransaction> action)
        {
            using var connection = GetConnection();
            using var transaction = connection.BeginTransaction();
            try
            {
                action(connection, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        private T WithTransaction<T>(Func<SqliteConnection, T> action)
        {
            using var connection = GetConnection();
            using var transaction = connection.BeginTransaction();
            try
            {
                var result = action(connection);
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private void InitializeDb()
        {

            if (WithConnection(connection =>
            {
                var checkTableQuery = $"SELECT name FROM sqlite_master WHERE type = 'table' AND name = {_primaryTableName};";
                using var checkTableCommand = new SqliteCommand(checkTableQuery, connection);
                return checkTableCommand.ExecuteScalar() != null;
            }))
                return;

            WithTransaction((connection, transaaction) =>
            {
                using var createPrimaryTableCommand = new SqliteCommand(
                    $@"
                        CREATE TABLE {_primaryTableName} (
                        PrimaryKey TEXT PRIMARY KEY,
                        KeyString TEXT NOT NULL,
                        Value TEXT NOT NULL
                     );", connection, transaaction);
                createPrimaryTableCommand.ExecuteNonQuery();

                using var createForeignKeyTableCommand = new SqliteCommand(
                    $@"
                        CREATE TABLE {_foreignKeyTableName} (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ForeignKey TEXT,
                        KeyString TEXT NOT NULL,
                        PrimaryKey TEXT,
                        FOREIGN KEY (PrimaryKey) REFERENCES {_primaryTableName}(PrimaryKey) ON DELETE CASCADE,
                        UNIQUE (ForeignKey, PrimaryKey)
                     );", connection, transaaction);
                createForeignKeyTableCommand.ExecuteNonQuery();
            });
        }

        public Task<Result<T, StorageResultReason>> Get<T>(StorageKey<T> key)
        {
            var keyString = StorageKeyConvert.Serialize(key);
            var result = WithConnection(connection =>
            {
                var query = $"SELECT Value FROM {_primaryTableName} WHERE PrimaryKey = @key";
                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@key", keyString);
                return command.ExecuteScalar();
            });

            if (result == null)
                return Task.FromResult(new Result<T, StorageResultReason>(StorageResultReason.NotFound));
            var resultString = result.ToString()
                ?? throw new JsonException("unable to deserialize result");
            var value = JsonConvert.DeserializeObject<T>(resultString, _serializerSettings)
                ?? throw new JsonException("unable to deserialize result");
            return Task.FromResult(new Result<T, StorageResultReason>(value));
        }

        public Task<bool> ContainsKey(StorageKey key)
        {
            var keyString = StorageKeyConvert.Serialize(key);
            var count = WithConnection(connection =>
            {
                string query = $"SELECT COUNT(1) FROM {_primaryTableName} WHERE PrimaryKey = @key";
                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@key", keyString);
                return command.ExecuteScalar();
            });
            if (count is not long longCount)
                throw new JsonException("unable to deserialize result");
            return Task.FromResult(longCount > 0);
        }

        public Task Set<T>(StorageKey<T> key, T value)
        {
            return Set(key, value, []);
        }

        public Task<Result<StorageResultReason>> Delete(StorageKey key)
        {
            var keyString = StorageKeyConvert.Serialize(key);
            var rowsAffected = WithConnection(connection =>
            {
                string query = $"DELETE FROM {_primaryTableName} WHERE PrimaryKey = @key";
                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@key", keyString);
                return command.ExecuteNonQuery();
            });
            if (rowsAffected == 0)
                return Task.FromResult(new Result<StorageResultReason>(StorageResultReason.NotFound));
            return Task.FromResult(new Result<StorageResultReason>());
        }

        public Task SetMany(List<(StorageKey Key, object? Value)> values)
        {
            InternalSetMany(values.Select(v => (v.Key, v.Value, new List<StorageKey>())));
            return Task.CompletedTask;
        }

        private void InternalSetMany(IEnumerable<(StorageKey Key, object? Value, List<StorageKey> ForeignKeys)> values)
        {
            WithTransaction((connection, transaction) =>
            {
                var primaryTableInsertCommand = connection.CreateCommand();
                primaryTableInsertCommand.Transaction = transaction;
                SqliteCommand? foreignKeyTableInsertCommand = null;
                var setForeignKeyTableInsertCommandParameters = new List<Action<StorageKey, StorageKey>>();

                var primaryTableInsertParameters = new List<(string Column, string ParameterName, Func<StorageKey, object?, List<StorageKey>, string> ParameterRetriever)>
                {
                    ("PrimaryKey", "primaryKey", (k, v, fk) => StorageKeyConvert.Serialize(k)),
                    ("Value", "value", (k, v, fk) => JsonConvert.SerializeObject(v, _serializerSettings))
                };
                if (_settings.StoreKeyStrings)
                    primaryTableInsertParameters.Add(("KeyString", "keyString", (k, v, fk) => k.ToString()));

                var setPrimaryTableInsertCommandParameters = new List<Action<StorageKey, object?, List<StorageKey>>>();
                foreach (var (column, parameterName, parameterRetriever) in primaryTableInsertParameters)
                {
                    var parameter = primaryTableInsertCommand.CreateParameter();
                    parameter.ParameterName = $"@{parameterName}";
                    primaryTableInsertCommand.Parameters.Add(parameter);
                    setPrimaryTableInsertCommandParameters.Add((k, v, pk) => parameter.Value = parameterRetriever(k, v, pk));
                }
                primaryTableInsertCommand.CommandText = $"INSERT INTO {_primaryTableName} ({string.Join(',', primaryTableInsertParameters.Select(p => p.Column))})"
                    + $" VALUES ({string.Join(',', primaryTableInsertParameters.Select(q => $"@{q.ParameterName}"))})"
                    + $" ON CONFLICT (PrimaryKey) DO UPDATE SET Value = excluded.Value";

                var foreignKeyTableInsertParameters = new List<(string Column, string ParameterName, Func<StorageKey, StorageKey, string> ParameterRetriever)>
                {
                    ("ForeignKey", "foreignKey", (fk, pk) => StorageKeyConvert.Serialize(fk)),
                    ("PrimaryKey", "primaryKey", (fk, pk) => StorageKeyConvert.Serialize(pk)),
                };
                if (_settings.StoreKeyStrings)
                    foreignKeyTableInsertParameters.Add(("KeyString", "keyString", (fk, pk) => fk.ToString()));

                foreach (var (key, value, foreignKeys) in values)
                {

                    foreach (var setFunc in setPrimaryTableInsertCommandParameters)
                        setFunc(key, value, foreignKeys);

                    primaryTableInsertCommand.ExecuteNonQuery();

                    foreach (var foreignKey in foreignKeys)
                    {
                        if (foreignKeyTableInsertCommand == null)
                        {
                            foreignKeyTableInsertCommand = connection.CreateCommand();
                            foreignKeyTableInsertCommand.Transaction = transaction;
                            foreach (var (column, parameterName, parameterRetriever) in foreignKeyTableInsertParameters)
                            {
                                var parameter = foreignKeyTableInsertCommand.CreateParameter();
                                parameter.ParameterName = $"@{parameterName}";
                                foreignKeyTableInsertCommand.Parameters.Add(parameter);
                                setForeignKeyTableInsertCommandParameters.Add((fk, pk) => parameter.Value = parameterRetriever(fk, pk));
                            }
                            foreignKeyTableInsertCommand.CommandText = $"INSERT INTO {_foreignKeyTableName} ({string.Join(',', foreignKeyTableInsertParameters.Select(p => p.Column))})"
                                + $" VALUES ({string.Join(',', foreignKeyTableInsertParameters.Select(q => $"@{q.ParameterName}"))})"
                                + $" ON CONFLICT (ForeignKey, PrimaryKey) DO NOTHING";
                        }

                        foreach (var setFunc in setForeignKeyTableInsertCommandParameters)
                            setFunc(foreignKey, key);

                        foreignKeyTableInsertCommand.ExecuteNonQuery();
                    }
                }
            });

        }

        public Task<List<(StorageKey<T> Key, T Value)>> GetMany<T>(StorageKey<T> foreignKey)
        {
            var keyString = StorageKeyConvert.Serialize(foreignKey);
            var results = WithConnection(connection =>
            {
                var query = $@"
                    SELECT {_primaryTableName}.PrimaryKey, {_primaryTableName}.Value
                    FROM {_foreignKeyTableName}
                    JOIN {_primaryTableName} ON {_foreignKeyTableName}.PrimaryKey = {_primaryTableName}.PrimaryKey
                    WHERE {_foreignKeyTableName}.ForeignKey = @key
                ";
                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@key", keyString);

                var reader = command.ExecuteReader();

                var results = new List<(StorageKey<T>, T)>();
                while (reader.Read())
                {
                    var primaryKeyString = reader.GetString(0);
                    var valueString = reader.GetString(1);

                    var value = JsonConvert.DeserializeObject<T>(valueString, _serializerSettings)
                        ?? throw new JsonException("unable to deserialize result");
                    var primaryKey = StorageKeyConvert.Deserialize<T>(primaryKeyString)
                        ?? throw new JsonException("unable to deserialize key");
                    results.Add((primaryKey, value));
                }

                return results;
            });

            return Task.FromResult(results);
        }

        public Task Set<T>(StorageKey<T> key, T value, List<StorageKey<T>> foreignKeys)
        {
            InternalSetMany([(key, value!, foreignKeys.Cast<StorageKey>().ToList())]);
            return Task.CompletedTask;
        }

        public Task<Result<int, StorageResultReason>> DeleteMany<T>(StorageKey<T> foreignKey)
        {
            var keyString = StorageKeyConvert.Serialize(foreignKey);

            var rowsAffected = WithConnection(connection =>
            {
                var query = $@"
                    DELETE FROM {_primaryTableName}
                    WHERE EXISTS (
                        SELECT 1
                        FROM {_foreignKeyTableName}
                        WHERE {_foreignKeyTableName}.PrimaryKey = {_primaryTableName}.PrimaryKey
                        AND {_foreignKeyTableName}.ForeignKey = @key
                    );
                ";

                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@key", keyString);
                return command.ExecuteNonQuery();
            });

            if (rowsAffected == 0)
                return Task.FromResult(new Result<int, StorageResultReason>(StorageResultReason.NotFound));
            return Task.FromResult(new Result<int, StorageResultReason>(rowsAffected));
        }

        public async Task<List<Result<T, StorageResultReason>>> GetMany<T>(List<StorageKey<T>> keys)
        {
            var results = await GetMany(keys.Cast<StorageKey>().ToList());
            return results.Select(r =>
            {
                if (r.IsSuccessful)
                    return new(TypeCoercer.Coerce<T>(r.Value));
                return new Result<T, StorageResultReason>(r.Reason);
            }).ToList();
        }

        public Task<List<Result<object?, StorageResultReason>>> GetMany(List<StorageKey> keys)
        {
            var results = WithConnection(connection =>
            {
                var query = $@"
                    SELECT Value
                    FROM {_primaryTableName}
                    WHERE PrimaryKey = @key;
                ";
                using var command = new SqliteCommand(query, connection);
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@key";
                command.Parameters.Add(parameter);

                var results = new List<Result<object?, StorageResultReason>>();
                foreach (var key in keys)
                {
                    var keyString = StorageKeyConvert.Serialize(key);
                    parameter.Value = keyString;
                    var result = command.ExecuteScalar();
                    if (result == null)
                    {
                        results.Add(new(StorageResultReason.NotFound));
                        continue;
                    }
                    var resultString = result.ToString()
                        ?? throw new JsonException("unable to deserialize result");
                    var value = JsonConvert.DeserializeObject(resultString, key.Type, _serializerSettings)
                        ?? throw new JsonException("unable to deserialize result");
                    results.Add(new(value));
                }

                return results;
            });

            return Task.FromResult(results);
        }
        public Task SetMany<T>(List<(StorageKey<T> Key, T? Value)> values)
            => SetMany(values.Select(kvp => ((StorageKey)kvp.Key, (object?)kvp.Value)).ToList());
    }
}

