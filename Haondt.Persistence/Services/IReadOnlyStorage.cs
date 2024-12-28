using Haondt.Core.Models;
using Haondt.Identity.StorageKey;

namespace Haondt.Persistence.Services
{
    public interface IReadOnlyStorage
    {
        Task<bool> ContainsKey(StorageKey primaryKey);

        Task<Result<T, StorageResultReason>> Get<T>(StorageKey<T> primaryKey) where T : notnull;
        Task<List<Result<object, StorageResultReason>>> GetMany(List<StorageKey> primaryKeys);
        Task<List<Result<T, StorageResultReason>>> GetMany<T>(List<StorageKey<T>> primaryKeys) where T : notnull;
        Task<List<(StorageKey<T> Key, T Value)>> GetManyByForeignKey<T>(StorageKey<T> foreignKey, int? limit = null, int? offset = null) where T : notnull;
        Task<long> CountManyByForeignKey<T>(StorageKey<T> foreignKey) where T : notnull;
        Task<List<StorageKey<T>>> GetForeignKeys<T>(StorageKey<T> primaryKey) where T : notnull;
    }
}
