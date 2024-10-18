﻿using Haondt.Core.Converters;
using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.MongoDb.Models;
using Haondt.Persistence.Services;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Haondt.Persistence.MongoDb.Services
{
    public class MongoDbStorage : IStorage
    {
        private readonly IMongoCollection<HaondtMongoDbDocument> _collection;
        private readonly IMongoQueryable<HaondtMongoDbDocument> _queryableCollection;

        public MongoDbStorage(
            string database,
            string collection,
            IMongoClient client)
        {
            _collection = client.GetDatabase(database)
                .GetCollection<HaondtMongoDbDocument>(collection);
            _queryableCollection = _collection.AsQueryable();
        }

        public Task<bool> ContainsKey(StorageKey key)
        {
            return _queryableCollection
                .Where(q => q.PrimaryKey == key)
                .AnyAsync();
        }

        public async Task<Result<StorageResultReason>> Delete(StorageKey key)
        {
            var result = await _collection.DeleteOneAsync(q => q.PrimaryKey == key);
            return result.DeletedCount == 0 ? new(StorageResultReason.NotFound) : new();
        }

        public async Task<Result<int, StorageResultReason>> DeleteMany<T>(StorageKey<T> foreignKey)
        {
            var result = await _collection.DeleteManyAsync(q => q.ForeignKeys.Any(fk => fk == foreignKey));
            return result.DeletedCount == 0 ? new(StorageResultReason.NotFound) : new(checked((int)result.DeletedCount));
        }

        public async Task<Result<T, StorageResultReason>> Get<T>(StorageKey<T> key)
        {
            var result = await _queryableCollection.Where(q => q.PrimaryKey == key)
                .ToListAsync();
            if (result.Count == 0)
                return new(StorageResultReason.NotFound);
            return new(TypeCoercer.Coerce<T>(result.First().Value));
        }

        public async Task<List<Result<object?, StorageResultReason>>> GetMany(List<StorageKey> keys)
        {
            var foundItems = await _queryableCollection.Where(q => keys.Contains(q.PrimaryKey)).ToListAsync();
            var foundItemsDict = foundItems.ToDictionary(d => d.PrimaryKey, d => d);
            return keys.Select(key =>
            {
                if (!foundItemsDict.TryGetValue(key, out var result))
                    return new Result<object?, StorageResultReason>(StorageResultReason.NotFound);
                return new(result.Value);
            }).ToList();
        }

        public async Task<List<Result<T, StorageResultReason>>> GetMany<T>(List<StorageKey<T>> keys)
        {
            var foundItems = await _queryableCollection.Where(q => keys.Contains(q.PrimaryKey)).ToListAsync();
            var foundItemsDict = foundItems.ToDictionary(d => d.PrimaryKey.As<T>(), d => d);
            return keys.Select(key =>
            {
                if (!foundItemsDict.TryGetValue(key, out var result))
                    return new Result<T, StorageResultReason>(StorageResultReason.NotFound);
                return new(TypeCoercer.Coerce<T>(result.Value));
            }).ToList();
        }

        public async Task<List<(StorageKey<T> Key, T Value)>> GetMany<T>(StorageKey<T> foreignKey)
        {
            var result = await _queryableCollection.Where(q => q.ForeignKeys.Any(fk => fk == foreignKey))
                .ToListAsync();
            return result
                .Select(q => (q.PrimaryKey.As<T>(), TypeCoercer.Coerce<T>(q.Value)))
                .ToList();
        }

        public Task Set<T>(StorageKey<T> key, T value, List<StorageKey<T>> foreignKeys)
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

        public Task Set<T>(StorageKey<T> key, T value)
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

        public Task SetMany(List<(StorageKey Key, object? Value)> values)
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

        public Task SetMany<T>(List<(StorageKey<T> Key, T? Value)> values)
            => SetMany(values.Select(kvp => ((StorageKey)kvp.Key, (object?)kvp.Value)).ToList());
    }
}