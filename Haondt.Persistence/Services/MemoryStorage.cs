using Haondt.Core.Models;
using Haondt.Identity.StorageKey;

namespace Haondt.Persistence.Services
{
    public class MemoryStorage : IStorage
    {
        protected readonly Dictionary<StorageKey, object> _storage = [];

        public Task<bool> ContainsKey(StorageKey key) => Task.FromResult(_storage.ContainsKey(key));
        public Task<Result<StorageResultReason>> Delete(StorageKey key)
        {
            return Task.FromResult<Result<StorageResultReason>>(_storage.Remove(key)
                ? new()
                : new(StorageResultReason.NotFound));
        }

        public Task<Result<T, StorageResultReason>> Get<T>(StorageKey<T> key)
        {
            if (!_storage.TryGetValue(key, out var value))
                return Task.FromResult<Result<T, StorageResultReason>>(new(StorageResultReason.NotFound));
            if (value is not T castedValue)
                throw new InvalidCastException($"Cannot convert {key} to type {typeof(T)}");
            return Task.FromResult<Result<T, StorageResultReason>>(new(castedValue));
        }

        public Task Set<T>(StorageKey<T> key, T value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            _storage[key] = value;
            return Task.CompletedTask;
        }
    }
}
