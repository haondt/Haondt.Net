using Haondt.Core.Models;
using Haondt.Identity.StorageKey;

namespace Haondt.Persistence.Services
{
    public interface IStorage
    {
        public Task<Result<T, StorageResultReason>> Get<T>(StorageKey<T> key);
        public Task<bool> ContainsKey(StorageKey key);
        public Task Set<T>(StorageKey<T> key, T value);
        public Task<Result<StorageResultReason>> Delete(StorageKey key);
    }
}
