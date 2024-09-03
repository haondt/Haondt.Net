using DotNext;
using Haondt.Identity.StorageKey;

namespace Haondt.Persistence.Services
{
    public class MemoryStorage : IStorage
    {
        protected readonly Dictionary<StorageKey, object> _storage = [];

        public Task<Result<bool>> ContainsKey(StorageKey key) => Task.FromResult(new Result<bool>(_storage.ContainsKey(key)));
        public Task<Optional<Exception>> Delete(StorageKey key)
        {
            _storage.Remove(key);
            return Task.FromResult<Optional<Exception>>(new());
        }

        public Task<Result<T>> Get<T>(StorageKey<T> key)
        {
            if (!_storage.TryGetValue(key, out var value))
                return Task.FromResult<Result<T>>(new(new KeyNotFoundException(StorageKeyConvert.Serialize(key))));
            if (value is not T castedValue)
                return Task.FromResult<Result<T>>(new (new InvalidCastException($"Cannot convert {key} to type {typeof(T)}")));
            return Task.FromResult<Result<T>>(new(castedValue));
        }

        public Task<Optional<Exception>> Set<T>(StorageKey<T> key, T value)
        {
            if (value is null)
                return Task.FromResult<Optional<Exception>>(new(new ArgumentNullException(nameof(value))));
            _storage[key] = value;
            return Task.FromResult<Optional<Exception>>(new ());
        }
    }
}
