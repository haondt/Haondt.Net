using Haondt.Core.Models;
using Haondt.Identity.StorageKey;

namespace Haondt.Persistence.Services
{
    public class TransientTransactionalBatchStorage(ITransactionalBatchOnlyStorage inner) : IStorage
    {
        public Task<bool> ContainsKey(StorageKey primaryKey) => inner.ContainsKey(primaryKey);
        public Task<Result<T, StorageResultReason>> Get<T>(StorageKey<T> primaryKey) where T : notnull => inner.Get(primaryKey);
        public Task<List<Result<object, StorageResultReason>>> GetMany(List<StorageKey> primaryKeys) => inner.GetMany(primaryKeys);
        public Task<List<Result<T, StorageResultReason>>> GetMany<T>(List<StorageKey<T>> primaryKeys) where T : notnull => inner.GetMany(primaryKeys);
        public Task<List<(StorageKey<T> Key, T Value)>> GetManyByForeignKey<T>(StorageKey<T> foreignKey) where T : notnull => inner.GetManyByForeignKey(foreignKey);
        public Task<List<StorageKey<T>>> GetForeignKeys<T>(StorageKey<T> primaryKey) where T : notnull => inner.GetForeignKeys<T>(primaryKey);

        public Task<StorageOperationBatchResult> PerformTransactionalBatch(List<StorageOperation> operations) => inner.PerformTransactionalBatch(operations);
        public Task<StorageOperationBatchResult> PerformTransactionalBatch<T>(List<StorageOperation<T>> operations) where T : notnull => inner.PerformTransactionalBatch(operations);

        public async Task<bool> Delete(StorageKey primaryKey)
        {
            var result = await inner.PerformTransactionalBatch(new List<StorageOperation>
            {
                new DeleteOperation
                {
                    Target = primaryKey
                }
            });
            return result.DeletedItems > 0;
        }

        public async Task<int> DeleteByForeignKey<T>(StorageKey<T> foreignKey) where T : notnull
        {
            var result = await inner.PerformTransactionalBatch(new List<StorageOperation<T>>
            {
                new DeleteByForeignKeyOperation<T>
                {
                    Target = foreignKey
                }
            });
            return result.DeletedItems;
        }


        public Task Set<T>(StorageKey<T> primaryKey, T value) where T : notnull
            => inner.PerformTransactionalBatch(new List<StorageOperation<T>>
            {
                new SetOperation<T>
                {
                    Target = primaryKey,
                    Value = value
                }
            });

        public Task Set<T>(StorageKey<T> primaryKey, T value, List<StorageKey<T>> addForeignKeys) where T : notnull
        {
            List<StorageOperation<T>> operations = new()
            {
                new SetOperation<T>{ Target = primaryKey, Value = value },
            };

            foreach (var fk in addForeignKeys)
                operations.Add(new AddForeignKeyOperation<T>
                {
                    ForeignKey = fk,
                    Target = primaryKey,
                });

            return inner.PerformTransactionalBatch(operations);
        }

        public Task SetMany(List<(StorageKey Key, object Value)> values)
            => inner.PerformTransactionalBatch(values
                .Select(t => new SetOperation
                {
                    Target = t.Key,
                    Value = t.Value
                }).Cast<StorageOperation>().ToList());

        public Task SetMany<T>(List<(StorageKey<T> Key, T Value)> values) where T : notnull
            => inner.PerformTransactionalBatch(values
                .Select(t => new SetOperation<T>
                {
                    Target = t.Key,
                    Value = t.Value
                }).Cast<StorageOperation<T>>().ToList());

    }
}
