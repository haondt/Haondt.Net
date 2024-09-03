using DotNext;
using Haondt.Identity.StorageKey;

namespace Haondt.Persistence.Services
{
    public interface IStorage
    {
        public Task<Result<T>> Get<T>(StorageKey<T> key);
        public Task<Result<bool>> ContainsKey(StorageKey key);
        public Task<Optional<Exception>> Set<T>(StorageKey<T> key, T value);
        public Task<Optional<Exception>> Delete(StorageKey key);
    }
}
