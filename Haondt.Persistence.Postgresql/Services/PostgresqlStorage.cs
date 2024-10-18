using Haondt.Core.Converters;
using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.Converters;
using Haondt.Persistence.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;

namespace Haondt.Persistence.Postgresql.Services
{
    public class PostgresqlStorage : IStorage
    {
        private readonly PostgresqlStorageSettings _settings;
        protected readonly JsonSerializerSettings _serializerSettings;
        protected readonly string _connectionString;
        protected readonly string _primaryTableName;
        protected readonly string _foreignKeyTableName;

        public PostgresqlStorage(IOptions<PostgresqlStorageSettings> options)
        {
            _settings = options.Value;
            _serializerSettings = new JsonSerializerSettings();
            _connectionString = new NpgsqlConnectionStringBuilder
            {
                Host = _settings.Host,
                Database = _settings.Database,
                Username = _settings.Username,
                Password = _settings.Password,
                Port = _settings.Port
            }.ToString();
            ConfigureSerializerSettings(_serializerSettings);
            _primaryTableName = SanitizeTableName(_settings.PrimaryTableName);
            _foreignKeyTableName = SanitizeTableName(_settings.ForeignKeyTableName);
        }

        protected virtual JsonSerializerSettings ConfigureSerializerSettings(JsonSerializerSettings settings)
        {
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            settings.Formatting = Formatting.None;
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Converters.Add(new GenericStorageKeyJsonConverter());
            return settings;
        }

        protected string SanitizeTableName(string tableName)
        {
            var sanitized = tableName.Replace("\"", "\"\"");
            return $"\"{sanitized}\"";
        }

        protected virtual async Task<NpgsqlConnection> GetConnectionAsync()
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        protected async Task WithConnectionAsync(Func<NpgsqlConnection, Task> action)
        {
            await using var connection = await GetConnectionAsync();
            await action(connection);
        }

        protected async Task<T> WithConnectionAsync<T>(Func<NpgsqlConnection, Task<T>> action)
        {
            await using var connection = await GetConnectionAsync();
            return await action(connection);
        }

        protected async Task WithTransactionAsync(Func<NpgsqlConnection, NpgsqlTransaction, Task> action)
        {
            await using var connection = await GetConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                await action(connection, transaction);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        protected async Task<T> WithTransactionAsync<T>(Func<NpgsqlConnection, NpgsqlTransaction, Task<T>> action)
        {
            await using var connection = await GetConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                var result = await action(connection, transaction);
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Result<T, StorageResultReason>> Get<T>(StorageKey<T> key)
        {
            var keyString = StorageKeyConvert.Serialize(key);
            var result = await WithConnectionAsync(async connection =>
            {
                var query = $"SELECT value FROM {_primaryTableName} WHERE PrimaryKey = @key";
                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@key", keyString);
                return await command.ExecuteScalarAsync();
            });

            if (result == null)
                return new Result<T, StorageResultReason>(StorageResultReason.NotFound);

            var value = JsonConvert.DeserializeObject<T>(result.ToString()!, _serializerSettings)
                ?? throw new JsonException("Unable to deserialize result");
            return new Result<T, StorageResultReason>(value);
        }

        public async Task<bool> ContainsKey(StorageKey key)
        {
            var keyString = StorageKeyConvert.Serialize(key);
            return await WithConnectionAsync(async connection =>
            {
                string query = $"SELECT EXISTS(SELECT 1 FROM {_primaryTableName} WHERE PrimaryKey = @key)";
                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@key", keyString);
                var result = await command.ExecuteScalarAsync();
                if (result is bool casted)
                    return casted;
                return false;
            });
        }

        public Task Set<T>(StorageKey<T> key, T value)
        {
            return Set(key, value, []);
        }

        public Task Set<T>(StorageKey<T> key, T value, List<StorageKey<T>> foreignKeys)
        {
            return InternalSetManyAsync([(key, value!, foreignKeys.Cast<StorageKey>().ToList())]);
        }
        public Task SetMany(List<(StorageKey Key, object? Value)> values)
        {
            return InternalSetManyAsync(values.Select(v => (v.Key, v.Value, new List<StorageKey>())));
        }

        protected async Task InternalSetManyAsync(IEnumerable<(StorageKey Key, object? Value, List<StorageKey> ForeignKeys)> values)
        {
            await WithTransactionAsync(async (connection, transaction) =>
            {
                foreach (var (key, value, foreignKeys) in values)
                {
                    var keyString = StorageKeyConvert.Serialize(key);
                    var valueJson = JsonConvert.SerializeObject(value, _serializerSettings);

                    var upsertQuery = $@"
                        INSERT INTO {_primaryTableName} (PrimaryKey, value)
                        VALUES (@key, @value::jsonb)
                        ON CONFLICT (PrimaryKey) 
                        DO UPDATE SET value = @value::jsonb;";
                    if (_settings.StoreKeyStrings)
                        upsertQuery = $@"
                            INSERT INTO {_primaryTableName} (PrimaryKey, KeyString, value)
                            VALUES (@key, @keyString, @value::jsonb)
                            ON CONFLICT (PrimaryKey) 
                            DO UPDATE SET value = @value::jsonb;";

                    await using var upsertCommand = new NpgsqlCommand(upsertQuery, connection, transaction);
                    upsertCommand.Parameters.AddWithValue("@key", keyString);
                    if (_settings.StoreKeyStrings)
                        upsertCommand.Parameters.AddWithValue("@keyString", key.ToString());
                    upsertCommand.Parameters.AddWithValue("@value", valueJson);
                    await upsertCommand.ExecuteNonQueryAsync();

                    if (foreignKeys.Any())
                    {
                        var foreignKeyQuery = $@"
                            INSERT INTO {_foreignKeyTableName} (ForeignKey, PrimaryKey)
                            VALUES (@foreignKey, @primaryKey)
                            ON CONFLICT (ForeignKey, PrimaryKey) DO NOTHING;";
                        if (_settings.StoreKeyStrings)
                            foreignKeyQuery = $@"
                            INSERT INTO {_foreignKeyTableName} (ForeignKey, KeyString, PrimaryKey)
                            VALUES (@foreignKey, @foreignKeyString, @primaryKey)
                            ON CONFLICT (ForeignKey, PrimaryKey) DO NOTHING;";

                        await using var foreignKeyCommand = new NpgsqlCommand(foreignKeyQuery, connection, transaction);
                        foreignKeyCommand.Parameters.AddWithValue("@primaryKey", keyString);

                        var foreignKeyParam = foreignKeyCommand.Parameters.Add("@foreignKey", NpgsqlTypes.NpgsqlDbType.Text);
                        NpgsqlParameter? foreignKeyStringParam = null;
                        if (_settings.StoreKeyStrings)
                            foreignKeyStringParam = foreignKeyCommand.Parameters.Add("@foreignKeyString", NpgsqlTypes.NpgsqlDbType.Text);

                        foreach (var foreignKey in foreignKeys)
                        {
                            foreignKeyParam.Value = StorageKeyConvert.Serialize(foreignKey);
                            if (foreignKeyStringParam != null)
                                foreignKeyStringParam.Value = foreignKey.ToString();
                            await foreignKeyCommand.ExecuteNonQueryAsync();
                        }
                    }
                }
            });
        }

        public async Task<List<(StorageKey<T> Key, T Value)>> GetMany<T>(StorageKey<T> foreignKey)
        {
            var keyString = StorageKeyConvert.Serialize(foreignKey);
            return await WithConnectionAsync(async connection =>
            {
                var query = $@"
                    SELECT p.PrimaryKey, p.value
                    FROM {_foreignKeyTableName} f
                    JOIN {_primaryTableName} p ON f.PrimaryKey = p.PrimaryKey
                    WHERE f.ForeignKey = @key";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@key", keyString);

                var results = new List<(StorageKey<T>, T)>();
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var primaryKeyString = reader.GetString(0);
                    var valueJson = reader.GetString(1);

                    var primaryKey = StorageKeyConvert.Deserialize<T>(primaryKeyString)
                        ?? throw new JsonException("Unable to deserialize key");
                    var value = JsonConvert.DeserializeObject<T>(valueJson, _serializerSettings)
                        ?? throw new JsonException("Unable to deserialize value");

                    results.Add((primaryKey, value));
                }

                return results;
            });
        }

        public async Task<Result<int, StorageResultReason>> DeleteMany<T>(StorageKey<T> foreignKey)
        {
            var keyString = StorageKeyConvert.Serialize(foreignKey);

            var rowsAffected = await WithTransactionAsync(async (connection, transaction) =>
            {
                var query = $@"
                    DELETE FROM {_primaryTableName}
                    WHERE PrimaryKey IN (
                        SELECT PrimaryKey
                        FROM {_foreignKeyTableName}
                        WHERE ForeignKey = @key
                    )";

                await using var command = new NpgsqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@key", keyString);
                return await command.ExecuteNonQueryAsync();
            });

            if (rowsAffected == 0)
                return new Result<int, StorageResultReason>(StorageResultReason.NotFound);
            return new Result<int, StorageResultReason>(rowsAffected);
        }

        public async Task<List<Result<T, StorageResultReason>>> GetMany<T>(List<StorageKey<T>> keys)
        {
            var results = await GetMany(keys.Cast<StorageKey>().ToList());
            return results.Select(r =>
            {
                if (r.IsSuccessful)
                    return new Result<T, StorageResultReason>(TypeCoercer.Coerce<T>(r.Value));
                return new Result<T, StorageResultReason>(r.Reason);
            }).ToList();
        }

        public async Task<List<Result<object?, StorageResultReason>>> GetMany(List<StorageKey> keys)
        {
            return await WithConnectionAsync(async connection =>
            {
                var query = $"SELECT PrimaryKey, value FROM {_primaryTableName} WHERE PrimaryKey = ANY(@keys)";
                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@keys", keys.Select(k => StorageKeyConvert.Serialize(k)).ToArray());

                var keyResults = new Dictionary<string, string>();
                await using (var reader = await command.ExecuteReaderAsync())
                    while (await reader.ReadAsync())
                    {
                        var primaryKey = reader.GetString(0);
                        var valueJson = reader.GetString(1);
                        keyResults[primaryKey] = valueJson;
                    }

                return keys.Select(key =>
                {
                    var keyString = StorageKeyConvert.Serialize(key);
                    if (!keyResults.TryGetValue(keyString, out var valueJson))
                        return new Result<object?, StorageResultReason>(StorageResultReason.NotFound);

                    var value = JsonConvert.DeserializeObject(valueJson, key.Type, _serializerSettings)
                        ?? throw new JsonException("Unable to deserialize result");
                    return new Result<object?, StorageResultReason>(value);
                }).ToList();
            });
        }

        public async Task<Result<StorageResultReason>> Delete(StorageKey key)
        {
            var keyString = StorageKeyConvert.Serialize(key);
            var rowsAffected = await WithConnectionAsync(async connection =>
            {
                string query = $"DELETE FROM {_primaryTableName} WHERE PrimaryKey = @key";
                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@key", keyString);
                return await command.ExecuteNonQueryAsync();
            });

            if (rowsAffected == 0)
                return new Result<StorageResultReason>(StorageResultReason.NotFound);
            return new Result<StorageResultReason>();
        }

        public Task SetMany<T>(List<(StorageKey<T> Key, T? Value)> values)
            => SetMany(values.Select(kvp => ((StorageKey)kvp.Key, (object?)kvp.Value)).ToList());
    }
}