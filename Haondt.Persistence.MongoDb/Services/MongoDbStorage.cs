using Haondt.Core.Converters;
using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.Exceptions;
using Haondt.Persistence.MongoDb.Models;
using Haondt.Persistence.Services;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Haondt.Persistence.MongoDb.Services
{
    public class MongoDbStorage : IStorage
    {
        protected readonly IMongoCollection<HaondtMongoDbDocument> _collection;
        protected readonly IMongoQueryable<HaondtMongoDbDocument> _queryableCollection;

        public MongoDbStorage(
            string database,
            string collection,
            IMongoClient client)
        {
            _collection = client.GetDatabase(database)
                .GetCollection<HaondtMongoDbDocument>(collection);
            _queryableCollection = _collection.AsQueryable();
        }

        public async Task Add<T>(StorageKey<T> primaryKey, T value) where T : notnull
        {
            try
            {

                await _collection.InsertOneAsync(new HaondtMongoDbDocument
                {
                    PrimaryKey = primaryKey,
                    Value = value
                });
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                    throw new StorageKeyExistsException(primaryKey, ex);
                throw;
            }
        }

        public async Task Add<T>(StorageKey<T> primaryKey, T value, List<StorageKey<T>> addForeignKeys) where T : notnull
        {
            try
            {
                await _collection.InsertOneAsync(new HaondtMongoDbDocument
                {
                    PrimaryKey = primaryKey,
                    Value = value,
                    ForeignKeys = addForeignKeys.Cast<StorageKey>().ToList()
                });
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                    throw new StorageKeyExistsException(primaryKey, ex);
                throw;
            }
        }

        public async Task AddMany(List<(StorageKey Key, object Value)> values)
        {
            try
            {
                await _collection.InsertManyAsync(values.Select(v => new HaondtMongoDbDocument
                {
                    PrimaryKey = v.Key,
                    Value = v.Value,
                }));
            }
            catch (MongoBulkWriteException<HaondtMongoDbDocument> ex)
            {
                if (ex.WriteErrors.Any(e => e.Category == ServerErrorCategory.DuplicateKey))
                    throw new StorageKeyExistsException(ex);
                throw;
            }
        }

        public async Task AddMany<T>(List<(StorageKey<T> Key, T Value)> values) where T : notnull
        {
            try
            {
                await _collection.InsertManyAsync(values.Select(v => new HaondtMongoDbDocument
                {
                    PrimaryKey = v.Key,
                    Value = v.Value,
                }));
            }
            catch (MongoBulkWriteException<HaondtMongoDbDocument> ex)
            {
                if (ex.WriteErrors.Any(e => e.Category == ServerErrorCategory.DuplicateKey))
                    throw new StorageKeyExistsException(ex);
                throw;
            }
        }

        public Task<bool> ContainsKey(StorageKey key)
        {
            return _queryableCollection
                .Where(q => q.PrimaryKey == key)
                .AnyAsync();
        }

        public async Task<bool> Delete(StorageKey key)
        {
            var result = await _collection.DeleteOneAsync(q => q.PrimaryKey == key);
            return result.DeletedCount > 0;
        }

        public async Task<int> DeleteByForeignKey<T>(StorageKey<T> foreignKey) where T : notnull
        {
            var result = await _collection.DeleteManyAsync(q => q.ForeignKeys.Any(fk => fk == foreignKey));
            return (int)result.DeletedCount;
        }

        public async Task<Result<T, StorageResultReason>> Get<T>(StorageKey<T> key) where T : notnull
        {
            var result = await _queryableCollection.Where(q => q.PrimaryKey == key)
                .ToListAsync();
            if (result.Count == 0)
                return new(StorageResultReason.NotFound);
            return new(TypeConverter.Coerce<T>(result.First().Value));
        }

        public async Task<List<StorageKey<T>>> GetForeignKeys<T>(StorageKey<T> primaryKey) where T : notnull
        {
            var result = await _queryableCollection.Where(q => q.PrimaryKey == primaryKey)
                .ToListAsync();
            if (result.Count == 0)
                return [];
            return result.First().ForeignKeys.Select(k => k.As<T>()).ToList();
        }

        public async Task<List<Result<object, StorageResultReason>>> GetMany(List<StorageKey> keys)
        {
            var foundItems = await _queryableCollection.Where(q => keys.Contains(q.PrimaryKey)).ToListAsync();
            var foundItemsDict = foundItems.ToDictionary(d => d.PrimaryKey, d => d);
            return keys.Select(key =>
            {
                if (!foundItemsDict.TryGetValue(key, out var result))
                    return new Result<object, StorageResultReason>(StorageResultReason.NotFound);
                return new(result.Value);
            }).ToList();
        }

        public async Task<List<Result<T, StorageResultReason>>> GetMany<T>(List<StorageKey<T>> keys) where T : notnull
        {
            var foundItems = await _queryableCollection.Where(q => keys.Contains(q.PrimaryKey)).ToListAsync();
            var foundItemsDict = foundItems.ToDictionary(d => d.PrimaryKey.As<T>(), d => d);
            return keys.Select(key =>
            {
                if (!foundItemsDict.TryGetValue(key, out var result))
                    return new Result<T, StorageResultReason>(StorageResultReason.NotFound);
                return new(TypeConverter.Coerce<T>(result.Value));
            }).ToList();
        }

        public async Task<List<(StorageKey<T> Key, T Value)>> GetManyByForeignKey<T>(StorageKey<T> foreignKey,
            int? limit = null, int? offset = null) where T : notnull
        {
            var query = _queryableCollection.Where(q => q.ForeignKeys.Any(fk => fk == foreignKey));

            if (offset.HasValue)
                query = query.Skip(offset.Value);
            if (limit.HasValue)
                query = query.Take(limit.Value);

            var result = await query
                .OrderBy(q => q.PrimaryKey)
                .ToListAsync();

            return result
                .Select(q => (q.PrimaryKey.As<T>(), TypeConverter.Coerce<T>(q.Value)))
                .ToList();
        }

        public Task<StorageOperationBatchResult> PerformTransactionalBatch(List<StorageOperation> operations)
        {
            throw new NotImplementedException();
        }

        public Task<StorageOperationBatchResult> PerformTransactionalBatch<T>(List<StorageOperation<T>> operations) where T : notnull
        {
            throw new NotImplementedException();
        }

        public Task Set<T>(StorageKey<T> key, T value, List<StorageKey<T>> foreignKeys) where T : notnull
        {
            var updateDefinition = Builders<HaondtMongoDbDocument>.Update
                .Set(d => d.Value, value)
                .SetOnInsert(d => d.PrimaryKey, key)
                .AddToSetEach(d => d.ForeignKeys, foreignKeys.Cast<StorageKey>());

            return _collection.FindOneAndUpdateAsync<HaondtMongoDbDocument>(
                d => d.PrimaryKey == key,
                updateDefinition,
                new FindOneAndUpdateOptions<HaondtMongoDbDocument, HaondtMongoDbDocument>
                {
                    IsUpsert = true
                });

        }

        public Task Set<T>(StorageKey<T> key, T value) where T : notnull
        {
            return _collection.FindOneAndReplaceAsync<HaondtMongoDbDocument>(d => d.PrimaryKey == key, new HaondtMongoDbDocument
            {
                PrimaryKey = key,
                Value = value
            }, new FindOneAndReplaceOptions<HaondtMongoDbDocument, HaondtMongoDbDocument>
            {
                IsUpsert = true
            });
        }

        public Task SetMany(List<(StorageKey Key, object Value)> values)
        {
            if (values.Count == 0)
                return Task.CompletedTask;

            var bulkOps = new List<WriteModel<HaondtMongoDbDocument>>();

            foreach (var (key, value) in values)
            {
                var filter = Builders<HaondtMongoDbDocument>.Filter.Eq(d => d.PrimaryKey, key);
                var replaceOneModel = new ReplaceOneModel<HaondtMongoDbDocument>(filter, new HaondtMongoDbDocument
                {
                    PrimaryKey = key,
                    Value = value
                })
                {
                    IsUpsert = true
                };

                bulkOps.Add(replaceOneModel);
            }

            return _collection.BulkWriteAsync(bulkOps);
        }

        public Task SetMany<T>(List<(StorageKey<T> Key, T Value)> values) where T : notnull
            => SetMany(values.Select(kvp => ((StorageKey)kvp.Key, (object)kvp.Value)).ToList());
    }
}
