using Haondt.Identity.StorageKey;

namespace Haondt.Persistence.Services
{
    public interface IStorage : ITransactionalBatchOnlyStorage
    {

        Task Set<T>(StorageKey<T> primaryKey, T value) where T : notnull;
        Task Set<T>(StorageKey<T> primaryKey, T value, List<StorageKey<T>> addForeignKeys) where T : notnull;
        Task SetMany(List<(StorageKey Key, object Value)> values);
        Task SetMany<T>(List<(StorageKey<T> Key, T Value)> values) where T : notnull;

        Task Add<T>(StorageKey<T> primaryKey, T value) where T : notnull;
        Task Add<T>(StorageKey<T> primaryKey, T value, List<StorageKey<T>> addForeignKeys) where T : notnull;
        Task AddMany(List<(StorageKey Key, object Value)> values);
        Task AddMany<T>(List<(StorageKey<T> Key, T Value)> values) where T : notnull;

        Task<bool> Delete(StorageKey primaryKey);
        Task<int> DeleteByForeignKey<T>(StorageKey<T> foreignKey) where T : notnull;
    }

}
