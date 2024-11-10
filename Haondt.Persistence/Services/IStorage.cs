using Haondt.Core.Models;
using Haondt.Identity.StorageKey;

namespace Haondt.Persistence.Services
{
    public interface IStorage
    {
        Task<bool> ContainsKey(StorageKey primaryKey);

        Task<Result<T, StorageResultReason>> Get<T>(StorageKey<T> primaryKey) where T : notnull;
        Task<List<Result<object, StorageResultReason>>> GetMany(List<StorageKey> primaryKeys);
        Task<List<Result<T, StorageResultReason>>> GetMany<T>(List<StorageKey<T>> primaryKeys) where T : notnull;
        Task<List<(StorageKey<T> Key, T Value)>> GetMany<T>(StorageKey<T> foreignKey) where T : notnull;

        Task Set<T>(StorageKey<T> primaryKey, T value) where T : notnull;
        Task Set<T>(StorageKey<T> primaryKey, T value, List<StorageKey<T>> addForeignKeys) where T : notnull;
        Task SetMany(List<(StorageKey Key, object Value)> values);
        Task SetMany<T>(List<(StorageKey<T> Key, T Value)> values) where T : notnull;

        Task<Result<StorageResultReason>> Delete(StorageKey primaryKey);
        Task<Result<int, StorageResultReason>> DeleteMany<T>(StorageKey<T> foreignKey) where T : notnull;
    }
}
