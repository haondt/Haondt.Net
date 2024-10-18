using Haondt.Core.Converters;
using Haondt.Core.Models;
using Haondt.Identity.StorageKey;

namespace Haondt.Persistence.Services
{
    public class MemoryEntry
    {
        public required object? Value { get; set; }
        public HashSet<StorageKey> ForeignKeys { get; set; } = [];
    }

    public class MemoryStorage : IStorage
    {
        protected readonly Dictionary<StorageKey, MemoryEntry> _storage = new();

        public Task<bool> ContainsKey(StorageKey key) => Task.FromResult(_storage.ContainsKey(key));

        public Task<Result<StorageResultReason>> Delete(StorageKey key)
        {
            if (_storage.Remove(key))
                return Task.FromResult(new Result<StorageResultReason>());
            return Task.FromResult(new Result<StorageResultReason>(StorageResultReason.NotFound));
        }

        public Task<Result<int, StorageResultReason>> DeleteMany<T>(StorageKey<T> foreignKey)
        {
            var keysToRemove = _storage.Where(kvp => kvp.Value.ForeignKeys.Contains(foreignKey))
                .Select(kvp => kvp.Key);
            var removed = 0;
            foreach (var key in keysToRemove)
            {
                _storage.Remove(key);
                removed++;
            }

            return Task.FromResult<Result<int, StorageResultReason>>(removed > 0 ? new(removed) : new(StorageResultReason.NotFound));
        }


        public Task<Result<T, StorageResultReason>> Get<T>(StorageKey<T> key)
        {
            if (_storage.TryGetValue(key, out var value))
                return Task.FromResult(new Result<T, StorageResultReason>(TypeCoercer.Coerce<T>(value.Value)));
            return Task.FromResult(new Result<T, StorageResultReason>(StorageResultReason.NotFound));
        }

        public Task<List<(StorageKey<T> Key, T Value)>> GetMany<T>(StorageKey<T> foreignKey)
        {
            return Task.FromResult(_storage
                .Where(kvp => kvp.Value.ForeignKeys.Contains(foreignKey))
                .Select(kvp => (kvp.Key.As<T>(), TypeCoercer.Coerce<T>(kvp.Value.Value)))
                .ToList());
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
            return Task.FromResult(keys.Select(k =>
            {
                if (_storage.TryGetValue(k, out var value))
                    return new(value.Value);
                return new Result<object?, StorageResultReason>(StorageResultReason.NotFound);
            }).ToList());
        }

        public Task Set<T>(StorageKey<T> key, T value, List<StorageKey<T>> foreignKeys)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

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

        public Task Set<T>(StorageKey<T> key, T value) => Set(key, value, []);

        public Task SetMany(List<(StorageKey Key, object? Value)> values)
        {
            foreach (var (key, value) in values)
                _storage[key] = new MemoryEntry { Value = value };
            return Task.CompletedTask;
        }

        public Task SetMany<T>(List<(StorageKey<T> Key, T? Value)> values)
            => SetMany(values.Select(kvp => ((StorageKey)kvp.Key, (object?)kvp.Value)).ToList());
    }
}
