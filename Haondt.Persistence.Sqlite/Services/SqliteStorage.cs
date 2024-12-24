using Haondt.Core.Converters;
using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.Converters;
using Haondt.Persistence.Exceptions;
using Haondt.Persistence.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Haondt.Persistence.Sqlite.Services
{
    public class SqliteStorage : ITransactionalBatchOnlyStorage
    {
        protected readonly SqliteStorageSettings _settings;
        protected readonly JsonSerializerSettings _serializerSettings;
        protected readonly string _connectionString;
        protected readonly string _primaryTableName;
        protected readonly string _foreignKeyTableName;

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
            // Escape any double quotes by replacing them with two double quotes
            var sanitized = tableName.Replace("\"", "\"\"");

            // Surround the sanitized table name with double quotes
            return $"\"{sanitized}\"";
        }

        protected virtual SqliteConnection GetConnection()
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

        protected void WithConnection(Action<SqliteConnection> action)
        {
            using var connection = GetConnection();
            action(connection);
        }

        protected T WithConnection<T>(Func<SqliteConnection, T> action)
        {
            using var connection = GetConnection();
            return action(connection);
        }

        protected void WithTransaction(Action<SqliteConnection, SqliteTransaction> action)
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
        protected T WithTransaction<T>(Func<SqliteConnection, T> action)
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

        protected virtual void InitializeDb()
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

        public Task<Result<T, StorageResultReason>> Get<T>(StorageKey<T> key) where T : notnull
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

        public Task<List<(StorageKey<T> Key, T Value)>> GetManyByForeignKey<T>(StorageKey<T> foreignKey) where T : notnull
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

        public async Task<List<Result<T, StorageResultReason>>> GetMany<T>(List<StorageKey<T>> keys) where T : notnull
        {
            var results = await GetMany(keys.Cast<StorageKey>().ToList());
            return results.Select(r =>
            {
                if (r.IsSuccessful)
                    return new(TypeConverter.Coerce<T>(r.Value));
                return new Result<T, StorageResultReason>(r.Reason);
            }).ToList();
        }

        public Task<List<Result<object, StorageResultReason>>> GetMany(List<StorageKey> keys)
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

                var results = new List<Result<object, StorageResultReason>>();
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

        private (SqliteCommand Command, List<SqliteParameter> Parameters) BuildCommand(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string commandText, List<string> parameters)
        {
            var command = new SqliteCommand(commandText, connection, transaction);
            var parametersList = new List<SqliteParameter>();
            foreach (var name in parameters)
                parametersList.Add(command.Parameters.Add(name, SqliteType.Text));

            return (command, parametersList);
        }

        private (Func<int> Command, Action Dispose, Action<StorageKey> SetParameters) GetDeleteForeignKeyCommand(SqliteConnection connection, SqliteTransaction transaction)
        {
            var query = $"DELETE FROM {_foreignKeyTableName} WHERE ForeignKey = @key";

            var (command, parameterList) = BuildCommand(
                connection,
                transaction,
                query,
                ["@key"]);
            var parameter = parameterList.Single();

            return (command.ExecuteNonQuery, command.Dispose, (k) =>
            {
                parameter.Value = StorageKeyConvert.Serialize(k);
            }
            );
        }
        private (Func<int> Command, Action Dispose, Action<StorageKey, StorageKey> SetParameters) GetRemoveForeignKeyCommand(SqliteConnection connection, SqliteTransaction transaction)
        {
            var query = $"DELETE FROM {_foreignKeyTableName} WHERE ForeignKey = @foreignKey AND PrimaryKey = @primaryKey";

            var (command, parameterList) = BuildCommand(
                connection,
                transaction,
                query,
                ["@foreignKey", "@primaryKey"]);

            return (command.ExecuteNonQuery, command.Dispose, (pk, fk) =>
            {
                parameterList[0].Value = StorageKeyConvert.Serialize(fk);
                parameterList[1].Value = StorageKeyConvert.Serialize(pk);
            }
            );
        }

        private (Func<int> Command, Action Dispose, Action<StorageKey> SetParameters) GetDeleteByForeignKeyCommand(SqliteConnection connection, SqliteTransaction transaction)
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

            var (command, parameterList) = BuildCommand(
                connection,
                transaction,
                query,
                ["@key"]);
            var parameter = parameterList.Single();

            return (command.ExecuteNonQuery, command.Dispose, (k) =>
            {
                parameter.Value = StorageKeyConvert.Serialize(k);
            }
            );
        }

        private (Func<int> Command, Action Dispose, Action<StorageKey> SetParameters) GetDeleteCommand(SqliteConnection connection, SqliteTransaction transaction)
        {
            var query = $"DELETE FROM {_primaryTableName} WHERE PrimaryKey = @key";

            var (command, parameterList) = BuildCommand(
                connection,
                transaction,
                query,
                ["@key"]);
            var parameter = parameterList.Single();

            return (command.ExecuteNonQuery, command.Dispose, (k) =>
            {
                parameter.Value = StorageKeyConvert.Serialize(k);
            }
            );
        }

        private (Action Command, Action Dispose, Action<StorageKey, object> SetParameters) GetUpsertCommand(SqliteConnection connection, SqliteTransaction transaction)
        {
            var columns = "PrimaryKey, Value";
            var parameters = new List<string>
            {
                "@key",
                "@value"
            };
            if (_settings.StoreKeyStrings)
            {
                columns += ", KeyString";
                parameters.Add("@keyString");
            }
            var parameterString = string.Join(", ", parameters);
            var upsertQuery = $@"
                INSERT INTO {_primaryTableName} ({columns})
                VALUES ({parameterString})
                ON CONFLICT (PrimaryKey) 
                DO UPDATE SET Value = excluded.Value";

            var (command, parameterList) = BuildCommand(
                connection,
                transaction,
                upsertQuery,
                parameters);

            var parameterDict = parameterList.Zip(parameters)
                .ToDictionary(t => t.Second, t => t.First);

            return (() => command.ExecuteNonQuery(), command.Dispose, (k, v) =>
            {
                parameterDict["@key"].Value = StorageKeyConvert.Serialize(k);
                parameterDict["@value"].Value = JsonConvert.SerializeObject(v, _serializerSettings);
                if (_settings.StoreKeyStrings)
                    parameterDict["@keyString"].Value = k.ToString();
            }
            );
        }
        private (Action Command, Action Dispose, Action<StorageKey, object> SetParameters) GetAddCommand(SqliteConnection connection, SqliteTransaction transaction)
        {
            var columns = "PrimaryKey, Value";
            var parameters = new List<string>
            {
                "@key",
                "@value"
            };
            if (_settings.StoreKeyStrings)
            {
                columns += ", KeyString";
                parameters.Add("@keyString");
            }
            var parameterString = string.Join(", ", parameters);
            var upsertQuery = $@"
                INSERT INTO {_primaryTableName} ({columns})
                VALUES ({parameterString})";

            var (command, parameterList) = BuildCommand(
                connection,
                transaction,
                upsertQuery,
                parameters);

            var parameterDict = parameterList.Zip(parameters)
                .ToDictionary(t => t.Second, t => t.First);

            return (() => command.ExecuteNonQuery(), command.Dispose, (k, v) =>
            {
                parameterDict["@key"].Value = StorageKeyConvert.Serialize(k);
                parameterDict["@value"].Value = JsonConvert.SerializeObject(v, _serializerSettings);
                if (_settings.StoreKeyStrings)
                    parameterDict["@keyString"].Value = k.ToString();
            }
            );
        }

        private (Action Command, Action Dispose, Action<StorageKey, StorageKey> SetParameters) GetAddForeignKeyCommand(SqliteConnection connection, SqliteTransaction transaction)
        {
            var columns = "ForeignKey, PrimaryKey";
            var parameters = new List<string>
            {
                "@foreignKey",
                "@primaryKey"
            };
            if (_settings.StoreKeyStrings)
            {
                columns += ", KeyString";
                parameters.Add("@keyString");
            }
            var parameterString = string.Join(", ", parameters);
            var query = $@"
                INSERT INTO {_foreignKeyTableName} ({columns})
                VALUES ({parameterString})
                ON CONFLICT (ForeignKey, PrimaryKey) DO NOTHING";

            var (command, parameterList) = BuildCommand(
                connection,
                transaction,
                query,
                parameters);
            var parameterDict = parameterList.Zip(parameters)
                .ToDictionary(t => t.Second, t => t.First);

            return (() => command.ExecuteNonQuery(), command.Dispose, (pk, fk) =>
            {
                parameterDict["@primaryKey"].Value = StorageKeyConvert.Serialize(pk);
                parameterDict["@foreignKey"].Value = StorageKeyConvert.Serialize(fk);
                if (_settings.StoreKeyStrings)
                    parameterDict["@keyString"].Value = fk.ToString();
            }
            );
        }

        public Task<StorageOperationBatchResult> PerformTransactionalBatch(List<StorageOperation> operations)
        {
            var result = new StorageOperationBatchResult();

            (Action Command, Action Dispose, Action<StorageKey, object> SetParameters)? upsertCommand = null;
            (Action Command, Action Dispose, Action<StorageKey, object> SetParameters)? addCommand = null;
            (Action Command, Action Dispose, Action<StorageKey, StorageKey> SetParameters)? addForeignKeyCommand = null;
            (Func<int> Command, Action Dispose, Action<StorageKey> SetParameters)? deleteCommand = null;
            (Func<int> Command, Action Dispose, Action<StorageKey> SetParameters)? deleteByForeignKeyCommand = null;
            (Func<int> Command, Action Dispose, Action<StorageKey> SetParameters)? deleteForeignKeyCommand = null;
            (Func<int> Command, Action Dispose, Action<StorageKey, StorageKey> SetParameters)? removeForeignKeyCommand = null;

            try
            {
                WithTransaction((connection, transaction) =>
                {
                    foreach (var operation in operations)
                    {
                        switch (operation)
                        {
                            case SetOperation setOp:
                                {
                                    upsertCommand ??= GetUpsertCommand(connection, transaction);
                                    upsertCommand.Value.SetParameters(setOp.Target, setOp.Value);
                                    upsertCommand.Value.Command();
                                    break;
                                }
                            case AddOperation addOp:
                                {
                                    addCommand ??= GetAddCommand(connection, transaction);
                                    addCommand.Value.SetParameters(addOp.Target, addOp.Value);
                                    try
                                    {
                                        addCommand.Value.Command();
                                    }
                                    catch (SqliteException ex)
                                    {
                                        if (ex.Message.Contains("UNIQUE constraint failed")
                                            && ex.Message.EndsWith(".PrimaryKey'."))
                                            throw new StorageKeyExistsException(addOp.Target, ex);
                                        throw;
                                    }
                                    break;
                                }
                            case AddForeignKeyOperation addFkOp:
                                {
                                    addForeignKeyCommand ??= GetAddForeignKeyCommand(connection, transaction);
                                    addForeignKeyCommand.Value.SetParameters(addFkOp.Target, addFkOp.ForeignKey);
                                    addForeignKeyCommand.Value.Command();
                                    break;
                                }
                            case DeleteOperation deleteOp:
                                {
                                    deleteCommand ??= GetDeleteCommand(connection, transaction);
                                    deleteCommand.Value.SetParameters(deleteOp.Target);
                                    var deleted = deleteCommand.Value.Command();
                                    result.DeletedItems += deleted;
                                    break;
                                }
                            case DeleteByForeignKeyOperation deleteByFkOp:
                                {
                                    deleteByForeignKeyCommand ??= GetDeleteByForeignKeyCommand(connection, transaction);
                                    deleteByForeignKeyCommand.Value.SetParameters(deleteByFkOp.Target);
                                    var deleted = deleteByForeignKeyCommand.Value.Command();
                                    result.DeletedItems += deleted;
                                    break;
                                }
                            case DeleteForeignKeyOperation deleteFkOp:
                                {
                                    deleteForeignKeyCommand ??= GetDeleteForeignKeyCommand(connection, transaction);
                                    deleteForeignKeyCommand.Value.SetParameters(deleteFkOp.Target);
                                    var deleted = deleteForeignKeyCommand.Value.Command();
                                    result.DeletedForeignKeys += deleted;
                                    break;
                                }
                            case CustomSqliteStorageOperation customOp:
                                {
                                    customOp.Execute(connection, transaction);
                                    break;
                                }
                            case RemoveForeignKeyOperation removeFkOp:
                                {
                                    removeForeignKeyCommand ??= GetRemoveForeignKeyCommand(connection, transaction);
                                    removeForeignKeyCommand.Value.SetParameters(removeFkOp.Target, removeFkOp.ForeignKey);
                                    result.DeletedForeignKeys += removeForeignKeyCommand.Value.Command();
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
                    upsertCommand.Value.Dispose();
                if (addCommand.HasValue)
                    addCommand.Value.Dispose();
                if (addForeignKeyCommand.HasValue)
                    addForeignKeyCommand.Value.Dispose();
                if (deleteCommand.HasValue)
                    deleteCommand.Value.Dispose();
                if (deleteByForeignKeyCommand.HasValue)
                    deleteByForeignKeyCommand.Value.Dispose();
                if (deleteForeignKeyCommand.HasValue)
                    deleteForeignKeyCommand.Value.Dispose();
                if (removeForeignKeyCommand.HasValue)
                    removeForeignKeyCommand.Value.Dispose();
            }

            return Task.FromResult(result);
        }

        public Task<StorageOperationBatchResult> PerformTransactionalBatch<T>(List<StorageOperation<T>> operations) where T : notnull
        {
            var result = new StorageOperationBatchResult();

            (Action Command, Action Dispose, Action<StorageKey, object> SetParameters)? upsertCommand = null;
            (Action Command, Action Dispose, Action<StorageKey, object> SetParameters)? addCommand = null;
            (Action Command, Action Dispose, Action<StorageKey, StorageKey> SetParameters)? addForeignKeyCommand = null;
            (Func<int> Command, Action Dispose, Action<StorageKey> SetParameters)? deleteCommand = null;
            (Func<int> Command, Action Dispose, Action<StorageKey> SetParameters)? deleteByForeignKeyCommand = null;
            (Func<int> Command, Action Dispose, Action<StorageKey> SetParameters)? deleteForeignKeyCommand = null;
            (Func<int> Command, Action Dispose, Action<StorageKey, StorageKey> SetParameters)? removeForeignKeyCommand = null;

            try
            {
                WithTransaction((connection, transaction) =>
                {
                    foreach (var operation in operations)
                    {
                        switch (operation)
                        {
                            case SetOperation<T> setOp:
                                {
                                    upsertCommand ??= GetUpsertCommand(connection, transaction);
                                    upsertCommand.Value.SetParameters(setOp.Target, setOp.Value);
                                    upsertCommand.Value.Command();
                                    break;
                                }
                            case AddOperation<T> addOp:
                                {
                                    addCommand ??= GetAddCommand(connection, transaction);
                                    addCommand.Value.SetParameters(addOp.Target, addOp.Value);
                                    try
                                    {
                                        addCommand.Value.Command();
                                    }
                                    catch (SqliteException ex)
                                    {
                                        if (ex.Message.Contains("UNIQUE constraint failed")
                                            && ex.Message.EndsWith(".PrimaryKey'."))
                                            throw new StorageKeyExistsException(addOp.Target, ex);
                                        throw;
                                    }
                                    break;
                                }
                            case AddForeignKeyOperation<T> addFkOp:
                                {
                                    addForeignKeyCommand ??= GetAddForeignKeyCommand(connection, transaction);
                                    addForeignKeyCommand.Value.SetParameters(addFkOp.Target, addFkOp.ForeignKey);
                                    addForeignKeyCommand.Value.Command();
                                    break;
                                }
                            case DeleteOperation<T> deleteOp:
                                {
                                    deleteCommand ??= GetDeleteCommand(connection, transaction);
                                    deleteCommand.Value.SetParameters(deleteOp.Target);
                                    var deleted = deleteCommand.Value.Command();
                                    result.DeletedItems += deleted;
                                    break;
                                }
                            case DeleteByForeignKeyOperation<T> deleteByFkOp:
                                {
                                    deleteByForeignKeyCommand ??= GetDeleteByForeignKeyCommand(connection, transaction);
                                    deleteByForeignKeyCommand.Value.SetParameters(deleteByFkOp.Target);
                                    var deleted = deleteByForeignKeyCommand.Value.Command();
                                    result.DeletedItems += deleted;
                                    break;
                                }
                            case DeleteForeignKeyOperation<T> deleteFkOp:
                                {
                                    deleteForeignKeyCommand ??= GetDeleteForeignKeyCommand(connection, transaction);
                                    deleteForeignKeyCommand.Value.SetParameters(deleteFkOp.Target);
                                    var deleted = deleteForeignKeyCommand.Value.Command();
                                    result.DeletedForeignKeys += deleted;
                                    break;
                                }
                            case CustomSqliteStorageOperation customOp:
                                {
                                    customOp.Execute(connection, transaction);
                                    break;
                                }
                            case RemoveForeignKeyOperation removeFkOp:
                                {
                                    removeForeignKeyCommand ??= GetRemoveForeignKeyCommand(connection, transaction);
                                    removeForeignKeyCommand.Value.SetParameters(removeFkOp.Target, removeFkOp.ForeignKey);
                                    var removed = removeForeignKeyCommand.Value.Command();
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
                    upsertCommand.Value.Dispose();
                if (addCommand.HasValue)
                    addCommand.Value.Dispose();
                if (addForeignKeyCommand.HasValue)
                    addForeignKeyCommand.Value.Dispose();
                if (deleteCommand.HasValue)
                    deleteCommand.Value.Dispose();
                if (deleteByForeignKeyCommand.HasValue)
                    deleteByForeignKeyCommand.Value.Dispose();
                if (deleteForeignKeyCommand.HasValue)
                    deleteForeignKeyCommand.Value.Dispose();
                if (removeForeignKeyCommand.HasValue)
                    removeForeignKeyCommand.Value.Dispose();
            }

            return Task.FromResult(result);
        }

        public Task<List<StorageKey<T>>> GetForeignKeys<T>(StorageKey<T> primaryKey) where T : notnull
        {
            var keyString = StorageKeyConvert.Serialize(primaryKey);
            var result = WithConnection(connection =>
            {
                var query = $@"
                    SELECT ForeignKey
                    FROM {_foreignKeyTableName}
                    WHERE PrimaryKey = @key";

                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@key", keyString);

                var results = new List<StorageKey<T>>();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var foreignKeyString = reader.GetString(0);
                    var foreignKey = StorageKeyConvert.Deserialize<T>(foreignKeyString)
                        ?? throw new JsonException("Unable to deserialize key");
                    results.Add(foreignKey);
                }

                return results;
            });

            return Task.FromResult(result);
        }
    }
}

