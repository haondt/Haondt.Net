﻿using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Json.Converters;
using Haondt.Persistence.Exceptions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Haondt.Persistence.Services
{
    public class DataObject
    {
        public Dictionary<string, DataLeaf> Values { get; set; } = [];
    }

    public class DataLeaf
    {
        public required JObject ValueContainer { get; set; }
        public HashSet<string> ForeignKeys { get; set; } = [];
    }

    public class ValueContainer<T> where T : notnull
    {
        public required T Value { get; set; }
    }
    public class ValueContainer
    {
        public required object Value { get; set; }
    }

    public class FileStorage : ITransactionalBatchOnlyStorage
    {
        protected readonly string _dataFile;
        protected readonly JsonSerializerSettings _serializerSettings;
        private readonly JsonSerializer _jObjectSerializer;
        protected DataObject? _dataCache;
        protected readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        public FileStorage(IOptions<HaondtFileStorageSettings> options)
        {
            _dataFile = options.Value.DataFile;
            _serializerSettings = new JsonSerializerSettings();
            ConfigureSerializerSettings(_serializerSettings);
            _jObjectSerializer = JsonSerializer.Create(_serializerSettings);
        }

        protected virtual JsonSerializerSettings ConfigureSerializerSettings(JsonSerializerSettings settings)
        {
            settings.TypeNameHandling = TypeNameHandling.None;
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            settings.Formatting = Formatting.Indented;
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Converters.Add(new GenericStorageKeyJsonConverter());
            return settings;
        }

        protected async Task TryAcquireSemaphoreAnd(Func<Task> func)
        {
            if (!await _semaphoreSlim.WaitAsync(1000))
                throw new Exception("Unable to acquire semaphore within the time limit");
            try { await func(); }
            finally { _semaphoreSlim.Release(); }
        }

        protected async Task<T> TryAcquireSemaphoreAnd<T>(Func<Task<T>> func)
        {
            if (!await _semaphoreSlim.WaitAsync(1000))
                throw new Exception("Unable to acquire semaphore within the time limit");
            try { return await func(); }
            finally { _semaphoreSlim.Release(); }
        }

        protected async Task<DataObject> GetDataAsync()
        {
            if (_dataCache != null)
                return _dataCache;

            if (!File.Exists(_dataFile))
            {
                _dataCache = new DataObject();
                return _dataCache;
            }

            using var reader = new StreamReader(_dataFile, new FileStreamOptions
            {
                Access = FileAccess.Read,
                BufferSize = 4096,
                Mode = FileMode.Open,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan
            });

            var text = await reader.ReadToEndAsync();

            _dataCache = JsonConvert.DeserializeObject<DataObject>(text, _serializerSettings) ?? new DataObject();
            return _dataCache;
        }

        protected async Task SetDataAsync(DataObject data)
        {
            using var writer = new StreamWriter(_dataFile, new FileStreamOptions
            {
                Access = FileAccess.Write,
                BufferSize = 4096,
                Mode = FileMode.Create,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan
            });

            var text = JsonConvert.SerializeObject(data, _serializerSettings);
            await writer.WriteAsync(text);
            _dataCache = data;
        }

        public Task<bool> ContainsKey(StorageKey key) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                return data.Values.ContainsKey(StorageKeyConvert.Serialize(key));
            });


        protected T ExtractContainerValue<T>(StorageKey<T> key, DataLeaf leaf) where T : notnull
        {
            return ExtractContainerValue<T>(key.ToString(), leaf);
        }
        protected T ExtractContainerValue<T>(string key, DataLeaf leaf) where T : notnull
        {
            var container = leaf.ValueContainer.ToObject<ValueContainer<T>>(_jObjectSerializer)
                ?? throw new InvalidCastException($"Cannot convert {key} to type {typeof(T)}");
            return container.Value;
        }
        protected object ExtractContainerValue(StorageKey key, DataLeaf leaf)
        {
            var genericType = typeof(ValueContainer<>).MakeGenericType(key.Type);
            var container = leaf.ValueContainer.ToObject(genericType, _jObjectSerializer)
                ?? throw new InvalidCastException($"Cannot convert {key} to type {key.Type}");
            var genericParameter = genericType.GetProperty(nameof(ValueContainer<object>.Value))!;
            return genericParameter.GetValue(container)!;
        }

        public Task<DetailedResult<T, StorageResultReason>> Get<T>(StorageKey<T> key) where T : notnull =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                var stringKey = StorageKeyConvert.Serialize(key);
                if (!data.Values.TryGetValue(stringKey, out var value))
                    return new(StorageResultReason.NotFound);
                return new DetailedResult<T, StorageResultReason>(ExtractContainerValue<T>(key, value));
            });


        public Task<List<DetailedResult<object, StorageResultReason>>> GetMany(List<StorageKey> primaryKeys) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                return primaryKeys.Select(k => data.Values.TryGetValue(StorageKeyConvert.Serialize(k), out var leaf)
                ? new DetailedResult<object, StorageResultReason>(ExtractContainerValue(k, leaf))
                : new(StorageResultReason.NotFound)).ToList();
            });

        public Task<List<DetailedResult<T, StorageResultReason>>> GetMany<T>(List<StorageKey<T>> primaryKeys) where T : notnull =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                return primaryKeys.Select(k => data.Values.TryGetValue(StorageKeyConvert.Serialize(k), out var leaf)
                ? new DetailedResult<T, StorageResultReason>(ExtractContainerValue<T>(k, leaf))
                : new(StorageResultReason.NotFound)).ToList();
            });


        public Task<List<(StorageKey<T> Key, T Value)>> GetManyByForeignKey<T>(StorageKey<T> foreignKey,
            int? limit = null, int? offset = null) where T : notnull =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                var foreignKeyString = StorageKeyConvert.Serialize(foreignKey);
                var result = data.Values
                    .OrderBy(x => x.Key)
                    .Where(kvp => kvp.Value.ForeignKeys.Contains(foreignKeyString))
                    .Select(kvp => (StorageKeyConvert.Deserialize<T>(kvp.Key),
                        ExtractContainerValue<T>(kvp.Key, kvp.Value)));

                if (offset.HasValue)
                    result = result.Skip(offset.Value);
                if (limit.HasValue)
                    result = result.Take(limit.Value);

                return result.ToList();
            });

        public Task<long> CountManyByForeignKey<T>(StorageKey<T> foreignKey) where T : notnull =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                var foreignKeyString = StorageKeyConvert.Serialize(foreignKey);
                var result = data.Values
                    .Where(kvp => kvp.Value.ForeignKeys.Contains(foreignKeyString))
                    .LongCount();

                return result;
            });


        public Task<StorageOperationBatchResult> PerformTransactionalBatch(List<StorageOperation> operations)
            => TryAcquireSemaphoreAnd(async () =>
            {
                var result = new StorageOperationBatchResult();
                var data = await GetDataAsync();

                foreach (var operation in operations)
                {
                    switch (operation)
                    {
                        case SetOperation setOp:
                            {
                                var primaryKeyString = StorageKeyConvert.Serialize(setOp.Target);
                                if (data.Values.TryGetValue(primaryKeyString, out var leaf))
                                {
                                    leaf.ValueContainer = JObject.FromObject(new ValueContainer { Value = setOp.Value }, _jObjectSerializer);
                                    data.Values[primaryKeyString] = leaf;
                                    break;
                                }

                                data.Values[primaryKeyString] = new DataLeaf
                                {
                                    ValueContainer = JObject.FromObject(new ValueContainer { Value = setOp.Value }, _jObjectSerializer),
                                };
                                break;
                            }
                        case AddOperation addOp:
                            {
                                var primaryKeyString = StorageKeyConvert.Serialize(addOp.Target);
                                if (data.Values.ContainsKey(primaryKeyString))
                                    throw new StorageKeyExistsException(addOp.Target);

                                data.Values[primaryKeyString] = new DataLeaf
                                {
                                    ValueContainer = JObject.FromObject(new ValueContainer { Value = addOp.Value }, _jObjectSerializer),
                                };
                                break;
                            }
                        case AddForeignKeyOperation addFkOp:
                            {
                                var primaryKeyString = StorageKeyConvert.Serialize(addFkOp.Target);
                                var foreignKeyString = StorageKeyConvert.Serialize(addFkOp.ForeignKey);
                                if (!data.Values.TryGetValue(primaryKeyString, out var leaf))
                                    throw new KeyNotFoundException($"No such key {addFkOp.Target}");

                                leaf.ForeignKeys.UnionWith(new HashSet<string> { foreignKeyString });
                                data.Values[primaryKeyString] = leaf;
                                break;
                            }
                        case DeleteOperation deleteOp:
                            {
                                var primaryKeyString = StorageKeyConvert.Serialize(deleteOp.Target);
                                if (data.Values.Remove(primaryKeyString))
                                    result.DeletedItems++;
                                break;
                            }
                        case DeleteByForeignKeyOperation deleteByFkOp:
                            {
                                var foreignKeyString = StorageKeyConvert.Serialize(deleteByFkOp.Target);
                                var primaryKeysToDelete = data.Values
                                    .Where(kvp => kvp.Value.ForeignKeys.Contains(foreignKeyString))
                                    .Select(kvp => kvp.Key)
                                    .ToList();

                                foreach (var primaryKey in primaryKeysToDelete)
                                    if (data.Values.Remove(primaryKey))
                                        result.DeletedItems++;
                                break;
                            }
                        case DeleteForeignKeyOperation deleteFkOp:
                            {
                                var foreignKeyString = StorageKeyConvert.Serialize(deleteFkOp.Target);
                                foreach (var kvp in data.Values)
                                    if (kvp.Value.ForeignKeys.Remove(foreignKeyString))
                                        result.DeletedForeignKeys++;
                                break;
                            }
                        case RemoveForeignKeyOperation removeFkOp:
                            {
                                var primaryKeyString = StorageKeyConvert.Serialize(removeFkOp.Target);
                                if (data.Values.TryGetValue(primaryKeyString, out var leaf))
                                {
                                    var foreignKeyString = StorageKeyConvert.Serialize(removeFkOp.ForeignKey);
                                    if (leaf.ForeignKeys.Remove(foreignKeyString))
                                    {
                                        data.Values[primaryKeyString] = leaf;
                                        result.DeletedForeignKeys++;
                                    }
                                }
                                break;
                            }
                        default:
                            throw new ArgumentException($"Unknown storage operation {operation.GetType()}");
                    }
                }

                await SetDataAsync(data);
                return result;
            });

        public Task<StorageOperationBatchResult> PerformTransactionalBatch<T>(List<StorageOperation<T>> operations) where T : notnull
            => TryAcquireSemaphoreAnd(async () =>
            {
                var result = new StorageOperationBatchResult();
                var data = await GetDataAsync();

                foreach (var operation in operations)
                {
                    switch (operation)
                    {
                        case SetOperation<T> setOp:
                            {
                                var primaryKeyString = StorageKeyConvert.Serialize(setOp.Target);
                                if (data.Values.TryGetValue(primaryKeyString, out var leaf))
                                {
                                    leaf.ValueContainer = JObject.FromObject(new ValueContainer<T> { Value = setOp.Value }, _jObjectSerializer);
                                    data.Values[primaryKeyString] = leaf;
                                    break;
                                }

                                data.Values[primaryKeyString] = new DataLeaf
                                {
                                    ValueContainer = JObject.FromObject(new ValueContainer<T> { Value = setOp.Value }, _jObjectSerializer),
                                };
                                break;
                            }
                        case AddOperation<T> addOp:
                            {
                                var primaryKeyString = StorageKeyConvert.Serialize(addOp.Target);
                                if (data.Values.ContainsKey(primaryKeyString))
                                    throw new StorageKeyExistsException(addOp.Target);

                                data.Values[primaryKeyString] = new DataLeaf
                                {
                                    ValueContainer = JObject.FromObject(new ValueContainer<T> { Value = addOp.Value }, _jObjectSerializer),
                                };
                                break;
                            }
                        case AddForeignKeyOperation<T> addFkOp:
                            {
                                var primaryKeyString = StorageKeyConvert.Serialize(addFkOp.Target);
                                var foreignKeyString = StorageKeyConvert.Serialize(addFkOp.ForeignKey);
                                if (!data.Values.TryGetValue(primaryKeyString, out var leaf))
                                    throw new KeyNotFoundException($"No such key {addFkOp.Target}");

                                leaf.ForeignKeys.UnionWith(new HashSet<string> { foreignKeyString });
                                data.Values[primaryKeyString] = leaf;
                                break;
                            }
                        case DeleteOperation<T> deleteOp:
                            {
                                var primaryKeyString = StorageKeyConvert.Serialize(deleteOp.Target);
                                if (data.Values.Remove(primaryKeyString))
                                    result.DeletedItems++;
                                break;
                            }
                        case DeleteByForeignKeyOperation<T> deleteByFkOp:
                            {
                                var foreignKeyString = StorageKeyConvert.Serialize(deleteByFkOp.Target);
                                var primaryKeysToDelete = data.Values
                                    .Where(kvp => kvp.Value.ForeignKeys.Contains(foreignKeyString))
                                    .Select(kvp => kvp.Key)
                                    .ToList();

                                foreach (var primaryKey in primaryKeysToDelete)
                                    if (data.Values.Remove(primaryKey))
                                        result.DeletedItems++;
                                break;
                            }
                        case DeleteForeignKeyOperation<T> deleteFkOp:
                            {
                                var foreignKeyString = StorageKeyConvert.Serialize(deleteFkOp.Target);
                                foreach (var kvp in data.Values)
                                    if (kvp.Value.ForeignKeys.Remove(foreignKeyString))
                                        result.DeletedForeignKeys++;
                                break;
                            }
                        case RemoveForeignKeyOperation<T> removeFkOp:
                            {
                                var primaryKeyString = StorageKeyConvert.Serialize(removeFkOp.Target);
                                if (data.Values.TryGetValue(primaryKeyString, out var leaf))
                                {
                                    var foreignKeyString = StorageKeyConvert.Serialize(removeFkOp.ForeignKey);
                                    if (leaf.ForeignKeys.Remove(foreignKeyString))
                                    {
                                        data.Values[primaryKeyString] = leaf;
                                        result.DeletedForeignKeys++;
                                    }
                                }
                                break;
                            }
                        default:
                            throw new ArgumentException($"Unknown storage operation {operation.GetType()}");
                    }
                }

                await SetDataAsync(data);
                return result;
            });

        public Task<List<StorageKey<T>>> GetForeignKeys<T>(StorageKey<T> primaryKey) where T : notnull =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                var stringKey = StorageKeyConvert.Serialize(primaryKey);
                if (!data.Values.TryGetValue(stringKey, out var value))
                    return [];
                return value.ForeignKeys.Select(fk => StorageKeyConvert.Deserialize<T>(fk)).ToList();
            });
    }
}
