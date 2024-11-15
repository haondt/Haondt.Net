using Haondt.Core.Converters;
using Haondt.Core.Models;
using Haondt.Identity.StorageKey;

namespace Haondt.Persistence.Services
{
    public class MemoryEntry
    {
        public required object Value { get; set; }
        public HashSet<StorageKey> ForeignKeys { get; set; } = [];
    }

    public class MemoryStorage : IStorage
    {
        protected readonly Dictionary<StorageKey, MemoryEntry> _storage = new();

        public Task<bool> ContainsKey(StorageKey key) => Task.FromResult(_storage.ContainsKey(key));

        public Task<bool> Delete(StorageKey key)
        {
            return Task.FromResult(_storage.Remove(key));
        }

        public Task<int> DeleteByForeignKey<T>(StorageKey<T> foreignKey) where T : notnull
        {
            var keysToRemove = _storage.Where(kvp => kvp.Value.ForeignKeys.Contains(foreignKey))
                .Select(kvp => kvp.Key);
            var removed = 0;
            foreach (var key in keysToRemove)
            {
                _storage.Remove(key);
                removed++;
            }
            return Task.FromResult(removed);
        }


        public Task<Result<T, StorageResultReason>> Get<T>(StorageKey<T> key) where T : notnull
        {
            if (_storage.TryGetValue(key, out var value))
                return Task.FromResult(new Result<T, StorageResultReason>(TypeConverter.Coerce<T>(value.Value)));
            return Task.FromResult(new Result<T, StorageResultReason>(StorageResultReason.NotFound));
        }

        public Task<List<(StorageKey<T> Key, T Value)>> GetManyByForeignKey<T>(StorageKey<T> foreignKey) where T : notnull
        {
            return Task.FromResult(_storage
                .Where(kvp => kvp.Value.ForeignKeys.Contains(foreignKey))
                .Select(kvp => (kvp.Key.As<T>(), TypeConverter.Coerce<T>(kvp.Value.Value)))
                .ToList());
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
            return Task.FromResult(keys.Select(k =>
            {
                if (_storage.TryGetValue(k, out var value))
                    return new(value.Value);
                return new Result<object, StorageResultReason>(StorageResultReason.NotFound);
            }).ToList());
        }

        public Task<StorageOperationBatchResult> PerformTransactionalBatch(List<StorageOperation> operations)
        {
            var newStorage = _storage.ToDictionary(kvp => kvp.Key, kvp => new MemoryEntry
            {
                Value = kvp.Value.Value,
                ForeignKeys = kvp.Value.ForeignKeys.ToHashSet()
            });

            var result = new StorageOperationBatchResult();

            foreach (var operation in operations)
            {
                switch (operation)
                {
                    case SetOperation setOp:
                        {
                            if (newStorage.TryGetValue(setOp.Target, out var memoryEntry))
                                memoryEntry.Value = setOp.Value;
                            else
                                newStorage[setOp.Target] = new MemoryEntry { Value = setOp.Value };
                            break;
                        }
                    case AddForeignKeyOperation addFkOp:
                        {
                            if (!newStorage.TryGetValue(addFkOp.Target, out var memoryEntry))
                                throw new ArgumentException($"No such primary key {addFkOp.Target}");
                            memoryEntry.ForeignKeys.Add(addFkOp.ForeignKey);
                            break;
                        }
                    case DeleteOperation deleteOp:
                        {
                            if (newStorage.Remove(deleteOp.Target))
                                result.DeletedItems++;
                            break;
                        }
                    case DeleteByForeignKeyOperation deleteByFkOp:
                        {

                            var keysToRemove = newStorage.Where(kvp => kvp.Value.ForeignKeys.Contains(deleteByFkOp.Target))
                                .Select(kvp => kvp.Key);
                            foreach (var key in keysToRemove)
                            {
                                newStorage.Remove(key);
                                result.DeletedItems++;
                            }
                            break;
                        }
                    case DeleteForeignKeyOperation deleteFkOp:
                        {
                            foreach (var kvp in newStorage)
                                if (kvp.Value.ForeignKeys.Remove(deleteFkOp.Target))
                                    result.DeletedForeignKeys++;
                            break;
                        }
                    default:
                        throw new ArgumentException($"Unknown storage operation {operation.GetType()}");
                }
            }

            var mainKeysToRemove = _storage.Keys.Where(k => !newStorage.ContainsKey(k));
            foreach (var key in mainKeysToRemove)
                _storage.Remove(key);
            foreach (var (key, value) in newStorage)
                _storage[key] = value;
            return Task.FromResult(result);
        }

        public Task<StorageOperationBatchResult> PerformTransactionalBatch<T>(List<StorageOperation<T>> operations) where T : notnull
        {
            var newStorage = _storage.ToDictionary(kvp => kvp.Key, kvp => new MemoryEntry
            {
                Value = kvp.Value,
                ForeignKeys = kvp.Value.ForeignKeys.ToHashSet()
            });

            var result = new StorageOperationBatchResult();

            foreach (var operation in operations)
            {
                switch (operation)
                {
                    case SetOperation<T> setOp:
                        {
                            if (newStorage.TryGetValue(setOp.Target, out var memoryEntry))
                                memoryEntry.Value = setOp.Value;
                            else
                                newStorage[setOp.Target] = new MemoryEntry { Value = setOp.Value };
                            break;
                        }
                    case AddForeignKeyOperation<T> addFkOp:
                        {
                            if (!newStorage.TryGetValue(addFkOp.Target, out var memoryEntry))
                                throw new ArgumentException($"No such primary key {addFkOp.Target}");
                            memoryEntry.ForeignKeys.Add(addFkOp.ForeignKey);
                            break;
                        }
                    case DeleteOperation<T> deleteOp:
                        {
                            if (newStorage.Remove(deleteOp.Target))
                                result.DeletedItems++;
                            break;
                        }
                    case DeleteByForeignKeyOperation<T> deleteByFkOp:
                        {

                            var keysToRemove = newStorage.Where(kvp => kvp.Value.ForeignKeys.Contains(deleteByFkOp.Target))
                                .Select(kvp => kvp.Key);
                            foreach (var key in keysToRemove)
                            {
                                newStorage.Remove(key);
                                result.DeletedItems++;
                            }
                            break;
                        }
                    case DeleteForeignKeyOperation<T> deleteFkOp:
                        {
                            foreach (var kvp in newStorage)
                                if (kvp.Value.ForeignKeys.Remove(deleteFkOp.Target))
                                    result.DeletedForeignKeys++;
                            break;
                        }
                    default:
                        throw new ArgumentException($"Unknown storage operation {operation.GetType()}");
                }
            }

            var mainKeysToRemove = _storage.Keys.Where(k => !newStorage.ContainsKey(k));
            foreach (var key in mainKeysToRemove)
                _storage.Remove(key);
            foreach (var (key, value) in newStorage)
                _storage[key] = value;
            return Task.FromResult(result);
        }

        public Task Set<T>(StorageKey<T> key, T value, List<StorageKey<T>> foreignKeys) where T : notnull
        {
            var foreignKeySet = foreignKeys.Cast<StorageKey>().ToHashSet();

            if (_storage.TryGetValue(key, out var existing))
                foreignKeySet.UnionWith(existing.ForeignKeys);

            _storage[key] = new MemoryEntry
            {
                Value = value,
                ForeignKeys = foreignKeySet
            };
            return Task.CompletedTask;
        }

        public Task Set<T>(StorageKey<T> key, T value) where T : notnull => Set(key, value, []);

        public Task SetMany(List<(StorageKey Key, object Value)> values)
        {
            foreach (var (key, value) in values)
                _storage[key] = new MemoryEntry { Value = value };
            return Task.CompletedTask;
        }

        public Task SetMany<T>(List<(StorageKey<T> Key, T Value)> values) where T : notnull
            => SetMany(values.Select(kvp => ((StorageKey)kvp.Key, (object)kvp.Value)).ToList());

        public Task<List<StorageKey<T>>> GetForeignKeys<T>(StorageKey<T> primaryKey) where T : notnull
        {
            if (_storage.TryGetValue(primaryKey, out var value))
                return Task.FromResult(value.ForeignKeys.Select(k => k.As<T>()).ToList());
            return Task.FromResult<List<StorageKey<T>>>([]);
        }
    }
}
