using Haondt.Identity.StorageKey;

namespace Haondt.Persistence.Services
{
    public interface ITransactionalBatchOnlyStorage : IReadOnlyStorage
    {
        Task<StorageOperationBatchResult> PerformTransactionalBatch(List<StorageOperation> operations);
        Task<StorageOperationBatchResult> PerformTransactionalBatch<T>(List<StorageOperation<T>> operations) where T : notnull;
    }

    public class StorageOperationBatchResult
    {
        public int DeletedItems { get; set; } = 0;
        public int DeletedForeignKeys { get; set; } = 0;
    }

    public abstract class StorageOperation
    {
        public required StorageKey Target { get; set; }
    }

    public abstract class StorageOperation<T> where T : notnull
    {
        public required StorageKey<T> Target { get; set; }
    }

    public class SetOperation : StorageOperation
    {
        public required object Value { get; set; }
    }

    public class SetOperation<T> : StorageOperation<T> where T : notnull
    {
        public required T Value { get; set; }
    }

    public class AddForeignKeyOperation : StorageOperation
    {
        public required StorageKey ForeignKey { get; set; }
    }

    public class AddForeignKeyOperation<T> : StorageOperation<T> where T : notnull
    {
        public required StorageKey<T> ForeignKey { get; set; }
    }

    public class DeleteOperation : StorageOperation { }

    public class DeleteOperation<T> : StorageOperation<T> where T : notnull { }

    public class DeleteByForeignKeyOperation : StorageOperation { }
    public class DeleteByForeignKeyOperation<T> : StorageOperation<T> where T : notnull { }

    public class DeleteForeignKeyOperation : StorageOperation { }

    public class DeleteForeignKeyOperation<T> : StorageOperation<T> where T : notnull { }
}
