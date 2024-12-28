using Haondt.Core.Converters;
using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.Converters;
using Haondt.Persistence.Exceptions;
using Haondt.Persistence.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;

namespace Haondt.Persistence.Postgresql.Services
{
    public class PostgresqlStorage : ITransactionalBatchOnlyStorage
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
            settings.TypeNameHandling = TypeNameHandling.None;
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

        public async Task<Result<T, StorageResultReason>> Get<T>(StorageKey<T> key) where T : notnull
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

        public async Task<List<(StorageKey<T> Key, T Value)>> GetManyByForeignKey<T>(StorageKey<T> foreignKey,
            int? limit = null, int? offset = null) where T : notnull
        {
            var limitStr = limit.HasValue ? $"LIMIT {limit.Value}" : string.Empty;
            var offsetStr = offset.HasValue ? $"OFFSET {offset.Value}" : string.Empty;
            var keyString = StorageKeyConvert.Serialize(foreignKey);
            return await WithConnectionAsync(async connection =>
            {
                var query = $@"
                    SELECT p.PrimaryKey, p.value
                    FROM {_foreignKeyTableName} f
                    JOIN {_primaryTableName} p ON f.PrimaryKey = p.PrimaryKey
                    WHERE f.ForeignKey = @key
                    ORDER BY PrimaryKey
                    {limitStr}
                    {offsetStr}
                ";

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

        public Task<long> CountManyByForeignKey<T>(StorageKey<T> foreignKey) where T : notnull
        {
            var keyString = StorageKeyConvert.Serialize(foreignKey);
            return WithConnectionAsync(async connection =>
            {
                var query = $@"
                    SELECT COUNT(1)
                    FROM {_foreignKeyTableName} f
                    JOIN {_primaryTableName} p ON f.PrimaryKey = p.PrimaryKey
                    WHERE f.ForeignKey = @key
                ";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@key", keyString);

                var results = new List<(StorageKey<T>, T)>();
                var result = await command.ExecuteScalarAsync();
                return TypeConverter.Coerce<long>(result ?? 0);
            });
        }

        public async Task<List<Result<T, StorageResultReason>>> GetMany<T>(List<StorageKey<T>> keys) where T : notnull
        {
            var results = await GetMany(keys.Cast<StorageKey>().ToList());
            return results.Select(r =>
            {
                if (r.IsSuccessful)
                    return new Result<T, StorageResultReason>(TypeConverter.Coerce<T>(r.Value));
                return new Result<T, StorageResultReason>(r.Reason);
            }).ToList();
        }

        public async Task<List<Result<object, StorageResultReason>>> GetMany(List<StorageKey> keys)
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
                        return new Result<object, StorageResultReason>(StorageResultReason.NotFound);

                    var value = JsonConvert.DeserializeObject(valueJson, key.Type, _serializerSettings)
                        ?? throw new JsonException("Unable to deserialize result");
                    return new Result<object, StorageResultReason>(value);
                }).ToList();
            });
        }


        private (NpgsqlCommand Command, List<NpgsqlParameter> Parameters) BuildCommand(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            string commandText, List<(string Name, NpgsqlDbType Type)> parameters)
        {
            var command = new NpgsqlCommand(commandText, connection, transaction);
            var parametersList = new List<NpgsqlParameter>();
            foreach (var (name, type) in parameters)
                parametersList.Add(command.Parameters.Add(name, type));

            return (command, parametersList);
        }

        private (Func<Task<int>> Command, Func<ValueTask> DisposeAsync, Action<StorageKey> SetParameters) GetDeleteForeignKeyCommand(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            var query = $"DELETE FROM {_foreignKeyTableName} WHERE ForeignKey = @key";

            var (command, parameterList) = BuildCommand(
                connection,
                transaction,
                query,
                [("@key", NpgsqlDbType.Text)]);
            var parameter = parameterList.Single();

            return (command.ExecuteNonQueryAsync, command.DisposeAsync, (k) =>
            {
                parameter.Value = StorageKeyConvert.Serialize(k);
            }
            );
        }
        private (Func<Task<int>> Command, Func<ValueTask> DisposeAsync, Action<StorageKey, StorageKey> SetParameters) GetRemoveForeignKeyCommand(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            var query = $"DELETE FROM {_foreignKeyTableName} WHERE ForeignKey = @foreignKey AND PrimaryKey = @primaryKey";

            var (command, parameterList) = BuildCommand(
                connection,
                transaction,
                query,
                [("@foreignKey", NpgsqlDbType.Text), ("@primaryKey", NpgsqlDbType.Text)]);

            return (command.ExecuteNonQueryAsync, command.DisposeAsync, (pk, fk) =>
            {
                parameterList[0].Value = StorageKeyConvert.Serialize(fk);
                parameterList[1].Value = StorageKeyConvert.Serialize(pk);
            }
            );
        }

        private (Func<Task<int>> Command, Func<ValueTask> DisposeAsync, Action<StorageKey> SetParameters) GetDeleteByForeignKeyCommand(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            var query = $@"
                DELETE FROM {_primaryTableName}
                WHERE PrimaryKey IN (
                    SELECT PrimaryKey
                    FROM {_foreignKeyTableName}
                    WHERE ForeignKey = @key
                )";

            var (command, parameterList) = BuildCommand(
                connection,
                transaction,
                query,
                [("@key", NpgsqlDbType.Text)]);
            var parameter = parameterList.Single();

            return (command.ExecuteNonQueryAsync, command.DisposeAsync, (k) =>
            {
                parameter.Value = StorageKeyConvert.Serialize(k);
            }
            );
        }

        private (Func<Task<int>> Command, Func<ValueTask> DisposeAsync, Action<StorageKey> SetParameters) GetDeleteCommand(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            var query = $"DELETE FROM {_primaryTableName} WHERE PrimaryKey = @key";

            var (command, parameterList) = BuildCommand(
                connection,
                transaction,
                query,
                [("@key", NpgsqlDbType.Text)]);
            var parameter = parameterList.Single();

            return (command.ExecuteNonQueryAsync, command.DisposeAsync, (k) =>
            {
                parameter.Value = StorageKeyConvert.Serialize(k);
            }
            );
        }

        private (Func<Task> Command, Func<ValueTask> DisposeAsync, Action<StorageKey, object> SetParameters) GetUpsertCommand(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            var columns = "PrimaryKey, value";
            var parameters = new List<(string, NpgsqlDbType)>
            {
                ("@key", NpgsqlDbType.Text),
                ("@value", NpgsqlDbType.Jsonb)
            };
            if (_settings.StoreKeyStrings)
            {
                columns += ", KeyString";
                parameters.Add(("@keyString", NpgsqlDbType.Text));
            }
            var parameterString = string.Join(", ", parameters.Select(p => p.Item2 == NpgsqlDbType.Jsonb ? $"{p.Item1}::jsonb" : p.Item1));
            var upsertQuery = $@"
                INSERT INTO {_primaryTableName} ({columns})
                VALUES ({parameterString})
                ON CONFLICT (PrimaryKey) 
                DO UPDATE SET value = @value::jsonb;";

            var (command, parameterList) = BuildCommand(
                connection,
                transaction,
                upsertQuery,
                parameters);

            var parameterDict = parameterList.Zip(parameters, (p, t) => (p, t))
                .ToDictionary(t => t.t.Item1, t => t.p);

            return (command.ExecuteNonQueryAsync, command.DisposeAsync, (k, v) =>
            {
                parameterDict["@key"].Value = StorageKeyConvert.Serialize(k);
                parameterDict["@value"].Value = JsonConvert.SerializeObject(v, _serializerSettings);
                if (_settings.StoreKeyStrings)
                    parameterDict["@keyString"].Value = k.ToString();
            }
            );
        }
        private (Func<Task> Command, Func<ValueTask> DisposeAsync, Action<StorageKey, object> SetParameters) GetAddCommand(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            var columns = "PrimaryKey, value";
            var parameters = new List<(string, NpgsqlDbType)>
            {
                ("@key", NpgsqlDbType.Text),
                ("@value", NpgsqlDbType.Jsonb)
            };
            if (_settings.StoreKeyStrings)
            {
                columns += ", KeyString";
                parameters.Add(("@keyString", NpgsqlDbType.Text));
            }
            var parameterString = string.Join(", ", parameters.Select(p => p.Item2 == NpgsqlDbType.Jsonb ? $"{p.Item1}::jsonb" : p.Item1));
            var upsertQuery = $@"
                INSERT INTO {_primaryTableName} ({columns})
                VALUES ({parameterString})";

            var (command, parameterList) = BuildCommand(
                connection,
                transaction,
                upsertQuery,
                parameters);

            var parameterDict = parameterList.Zip(parameters, (p, t) => (p, t))
                .ToDictionary(t => t.t.Item1, t => t.p);

            return (command.ExecuteNonQueryAsync, command.DisposeAsync, (k, v) =>
            {
                parameterDict["@key"].Value = StorageKeyConvert.Serialize(k);
                parameterDict["@value"].Value = JsonConvert.SerializeObject(v, _serializerSettings);
                if (_settings.StoreKeyStrings)
                    parameterDict["@keyString"].Value = k.ToString();
            }
            );
        }

        private (Func<Task> Command, Func<ValueTask> DisposeAsync, Action<StorageKey, StorageKey> SetParameters) GetAddForeignKeyCommand(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            var columns = "ForeignKey, PrimaryKey";
            var parameters = new List<(string, NpgsqlDbType)>
            {
                ("@foreignKey", NpgsqlDbType.Text),
                ("@primaryKey", NpgsqlDbType.Text)
            };
            if (_settings.StoreKeyStrings)
            {
                columns += ", KeyString";
                parameters.Add(("@keyString", NpgsqlDbType.Text));
            }
            var parameterString = string.Join(", ", parameters.Select(p => p.Item2 == NpgsqlDbType.Jsonb ? $"{p.Item1}::jsonb" : p.Item1));
            var query = $@"
                INSERT INTO {_foreignKeyTableName} ({columns})
                VALUES ({parameterString})
                ON CONFLICT (ForeignKey, PrimaryKey) DO NOTHING;";

            var (command, parameterList) = BuildCommand(
                connection,
                transaction,
                query,
                parameters);
            var parameterDict = parameterList.Zip(parameters, (p, t) => (p, t))
                .ToDictionary(t => t.t.Item1, t => t.p);

            return (command.ExecuteNonQueryAsync, command.DisposeAsync, (pk, fk) =>
            {
                parameterDict["@primaryKey"].Value = StorageKeyConvert.Serialize(pk);
                parameterDict["@foreignKey"].Value = StorageKeyConvert.Serialize(fk);
                if (_settings.StoreKeyStrings)
                    parameterDict["@keyString"].Value = fk.ToString();
            }
            );
        }

        public async Task<StorageOperationBatchResult> PerformTransactionalBatch(List<StorageOperation> operations)
        {
            var result = new StorageOperationBatchResult();

            (Func<Task> Command, Func<ValueTask> DisposeAsync, Action<StorageKey, object> SetParameters)? upsertCommand = null;
            (Func<Task> Command, Func<ValueTask> DisposeAsync, Action<StorageKey, object> SetParameters)? addCommand = null;
            (Func<Task> Command, Func<ValueTask> DisposeAsync, Action<StorageKey, StorageKey> SetParameters)? addForeignKeyCommand = null;
            (Func<Task<int>> Command, Func<ValueTask> DisposeAsync, Action<StorageKey> SetParameters)? deleteCommand = null;
            (Func<Task<int>> Command, Func<ValueTask> DisposeAsync, Action<StorageKey> SetParameters)? deleteByForeignKeyCommand = null;
            (Func<Task<int>> Command, Func<ValueTask> DisposeAsync, Action<StorageKey> SetParameters)? deleteForeignKeyCommand = null;
            (Func<Task<int>> Command, Func<ValueTask> DisposeAsync, Action<StorageKey, StorageKey> SetParameters)? removeForeignKeyCommand = null;

            try
            {
                await WithTransactionAsync(async (connection, transaction) =>
                {
                    foreach (var operation in operations)
                    {
                        switch (operation)
                        {
                            case SetOperation setOp:
                                {
                                    upsertCommand ??= GetUpsertCommand(connection, transaction);
                                    upsertCommand.Value.SetParameters(setOp.Target, setOp.Value);
                                    await upsertCommand.Value.Command();
                                    break;
                                }
                            case AddOperation addOp:
                                {
                                    addCommand ??= GetAddCommand(connection, transaction);
                                    addCommand.Value.SetParameters(addOp.Target, addOp.Value);
                                    try
                                    {
                                        await addCommand.Value.Command();
                                    }
                                    catch (NpgsqlException ex)
                                    {
                                        if (ex.Message.Contains("duplicate key value violates unique constraint"))
                                            throw new StorageKeyExistsException(addOp.Target, ex);
                                        throw;
                                    }
                                    break;
                                }
                            case AddForeignKeyOperation addFkOp:
                                {
                                    addForeignKeyCommand ??= GetAddForeignKeyCommand(connection, transaction);
                                    addForeignKeyCommand.Value.SetParameters(addFkOp.Target, addFkOp.ForeignKey);
                                    await addForeignKeyCommand.Value.Command();
                                    break;
                                }
                            case DeleteOperation deleteOp:
                                {
                                    deleteCommand ??= GetDeleteCommand(connection, transaction);
                                    deleteCommand.Value.SetParameters(deleteOp.Target);
                                    var deleted = await deleteCommand.Value.Command();
                                    result.DeletedItems += deleted;
                                    break;
                                }
                            case DeleteByForeignKeyOperation deleteByFkOp:
                                {
                                    deleteByForeignKeyCommand ??= GetDeleteByForeignKeyCommand(connection, transaction);
                                    deleteByForeignKeyCommand.Value.SetParameters(deleteByFkOp.Target);
                                    var deleted = await deleteByForeignKeyCommand.Value.Command();
                                    result.DeletedItems += deleted;
                                    break;
                                }
                            case DeleteForeignKeyOperation deleteFkOp:
                                {
                                    deleteForeignKeyCommand ??= GetDeleteForeignKeyCommand(connection, transaction);
                                    deleteForeignKeyCommand.Value.SetParameters(deleteFkOp.Target);
                                    var deleted = await deleteForeignKeyCommand.Value.Command();
                                    result.DeletedForeignKeys += deleted;
                                    break;
                                }
                            case RemoveForeignKeyOperation removeFkOp:
                                {
                                    removeForeignKeyCommand ??= GetRemoveForeignKeyCommand(connection, transaction);
                                    removeForeignKeyCommand.Value.SetParameters(removeFkOp.Target, removeFkOp.ForeignKey);
                                    var removed = await removeForeignKeyCommand.Value.Command();
                                    result.DeletedForeignKeys += removed;
                                    break;
                                }
                            default:
                                throw new ArgumentException($"Unknown storage operation {operation.GetType()}");
                        }
                    }
                });
            }
            finally
            {
                if (upsertCommand.HasValue)
                    await upsertCommand.Value.DisposeAsync();
                if (addCommand.HasValue)
                    await addCommand.Value.DisposeAsync();
                if (addForeignKeyCommand.HasValue)
                    await addForeignKeyCommand.Value.DisposeAsync();
                if (deleteCommand.HasValue)
                    await deleteCommand.Value.DisposeAsync();
                if (deleteByForeignKeyCommand.HasValue)
                    await deleteByForeignKeyCommand.Value.DisposeAsync();
                if (deleteForeignKeyCommand.HasValue)
                    await deleteForeignKeyCommand.Value.DisposeAsync();
                if (removeForeignKeyCommand.HasValue)
                    await removeForeignKeyCommand.Value.DisposeAsync();
            }

            return result;
        }

        public async Task<StorageOperationBatchResult> PerformTransactionalBatch<T>(List<StorageOperation<T>> operations) where T : notnull
        {
            var result = new StorageOperationBatchResult();

            (Func<Task> Command, Func<ValueTask> DisposeAsync, Action<StorageKey, object> SetParameters)? upsertCommand = null;
            (Func<Task> Command, Func<ValueTask> DisposeAsync, Action<StorageKey, object> SetParameters)? addCommand = null;
            (Func<Task> Command, Func<ValueTask> DisposeAsync, Action<StorageKey, StorageKey> SetParameters)? addForeignKeyCommand = null;
            (Func<Task<int>> Command, Func<ValueTask> DisposeAsync, Action<StorageKey> SetParameters)? deleteCommand = null;
            (Func<Task<int>> Command, Func<ValueTask> DisposeAsync, Action<StorageKey> SetParameters)? deleteByForeignKeyCommand = null;
            (Func<Task<int>> Command, Func<ValueTask> DisposeAsync, Action<StorageKey> SetParameters)? deleteForeignKeyCommand = null;
            (Func<Task<int>> Command, Func<ValueTask> DisposeAsync, Action<StorageKey, StorageKey> SetParameters)? removeForeignKeyCommand = null;

            try
            {
                await WithTransactionAsync(async (connection, transaction) =>
                {
                    foreach (var operation in operations)
                    {
                        switch (operation)
                        {
                            case SetOperation<T> setOp:
                                {
                                    upsertCommand ??= GetUpsertCommand(connection, transaction);
                                    upsertCommand.Value.SetParameters(setOp.Target, setOp.Value);
                                    await upsertCommand.Value.Command();
                                    break;
                                }
                            case AddOperation<T> addOp:
                                {
                                    addCommand ??= GetAddCommand(connection, transaction);
                                    addCommand.Value.SetParameters(addOp.Target, addOp.Value);
                                    try
                                    {
                                        await addCommand.Value.Command();
                                    }
                                    catch (NpgsqlException ex)
                                    {
                                        if (ex.Message.Contains("duplicate key value violates unique constraint"))
                                            throw new StorageKeyExistsException(addOp.Target, ex);
                                        throw;
                                    }
                                    break;
                                }
                            case AddForeignKeyOperation<T> addFkOp:
                                {
                                    addForeignKeyCommand ??= GetAddForeignKeyCommand(connection, transaction);
                                    addForeignKeyCommand.Value.SetParameters(addFkOp.Target, addFkOp.ForeignKey);
                                    await addForeignKeyCommand.Value.Command();
                                    break;
                                }
                            case DeleteOperation<T> deleteOp:
                                {
                                    deleteCommand ??= GetDeleteCommand(connection, transaction);
                                    deleteCommand.Value.SetParameters(deleteOp.Target);
                                    var deleted = await deleteCommand.Value.Command();
                                    result.DeletedItems += deleted;
                                    break;
                                }
                            case DeleteByForeignKeyOperation<T> deleteByFkOp:
                                {
                                    deleteByForeignKeyCommand ??= GetDeleteByForeignKeyCommand(connection, transaction);
                                    deleteByForeignKeyCommand.Value.SetParameters(deleteByFkOp.Target);
                                    var deleted = await deleteByForeignKeyCommand.Value.Command();
                                    result.DeletedItems += deleted;
                                    break;
                                }
                            case DeleteForeignKeyOperation<T> deleteFkOp:
                                {
                                    deleteForeignKeyCommand ??= GetDeleteForeignKeyCommand(connection, transaction);
                                    deleteForeignKeyCommand.Value.SetParameters(deleteFkOp.Target);
                                    var deleted = await deleteForeignKeyCommand.Value.Command();
                                    result.DeletedForeignKeys += deleted;
                                    break;
                                }
                            case RemoveForeignKeyOperation<T> removeFkOp:
                                {
                                    removeForeignKeyCommand ??= GetRemoveForeignKeyCommand(connection, transaction);
                                    removeForeignKeyCommand.Value.SetParameters(removeFkOp.Target, removeFkOp.ForeignKey);
                                    var removed = await removeForeignKeyCommand.Value.Command();
                                    result.DeletedForeignKeys += removed;
                                    break;
                                }
                            default:
                                throw new ArgumentException($"Unknown storage operation {operation.GetType()}");
                        }
                    }
                });
            }
            finally
            {
                if (upsertCommand.HasValue)
                    await upsertCommand.Value.DisposeAsync();
                if (addCommand.HasValue)
                    await addCommand.Value.DisposeAsync();
                if (addForeignKeyCommand.HasValue)
                    await addForeignKeyCommand.Value.DisposeAsync();
                if (deleteCommand.HasValue)
                    await deleteCommand.Value.DisposeAsync();
                if (deleteByForeignKeyCommand.HasValue)
                    await deleteByForeignKeyCommand.Value.DisposeAsync();
                if (deleteForeignKeyCommand.HasValue)
                    await deleteForeignKeyCommand.Value.DisposeAsync();
                if (removeForeignKeyCommand.HasValue)
                    await removeForeignKeyCommand.Value.DisposeAsync();
            }

            return result;
        }

        public async Task<List<StorageKey<T>>> GetForeignKeys<T>(StorageKey<T> primaryKey) where T : notnull
        {
            var keyString = StorageKeyConvert.Serialize(primaryKey);
            return await WithConnectionAsync(async connection =>
            {
                var query = $@"
                    SELECT ForeignKey
                    FROM {_foreignKeyTableName}
                    WHERE PrimaryKey = @key";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@key", keyString);

                var results = new List<StorageKey<T>>();
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var primaryKeyString = reader.GetString(0);
                    var primaryKey = StorageKeyConvert.Deserialize<T>(primaryKeyString)
                        ?? throw new JsonException("Unable to deserialize key");
                    results.Add(primaryKey);
                }

                return results;
            });
        }
    }
}