using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.Converters;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Haondt.Persistence.Services
{
    public class DataObject
    {
        public Dictionary<string, DataLeaf> Values { get; set; } = [];
    }

    public class DataLeaf
    {
        public required JObject ValueContainer { get; set; }
        public HashSet<string> ForeignKeys { get; set; } = [];
    }

    public class ValueContainer<T> where T : notnull
    {
        public required T Value { get; set; }
    }
    public class ValueContainer
    {
        public required object Value { get; set; }
    }

    public class FileStorage : IStorage
    {
        protected readonly string _dataFile;
        protected readonly JsonSerializerSettings _serializerSettings;
        private readonly JsonSerializer _jObjectSerializer;
        protected DataObject? _dataCache;
        protected readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        public FileStorage(IOptions<HaondtFileStorageSettings> options)
        {
            _dataFile = options.Value.DataFile;
            _serializerSettings = new JsonSerializerSettings();
            ConfigureSerializerSettings(_serializerSettings);
            _jObjectSerializer = JsonSerializer.Create(_serializerSettings);
        }

        protected virtual JsonSerializerSettings ConfigureSerializerSettings(JsonSerializerSettings settings)
        {
            settings.TypeNameHandling = TypeNameHandling.None;
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            settings.Formatting = Formatting.Indented;
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Converters.Add(new GenericStorageKeyJsonConverter());
            return settings;
        }

        protected async Task TryAcquireSemaphoreAnd(Func<Task> func)
        {
            if (!await _semaphoreSlim.WaitAsync(1000))
                throw new Exception("Unable to acquire semaphore within the time limit");
            try { await func(); }
            finally { _semaphoreSlim.Release(); }
        }

        protected async Task<T> TryAcquireSemaphoreAnd<T>(Func<Task<T>> func)
        {
            if (!await _semaphoreSlim.WaitAsync(1000))
                throw new Exception("Unable to acquire semaphore within the time limit");
            try { return await func(); }
            finally { _semaphoreSlim.Release(); }
        }

        protected async Task<DataObject> GetDataAsync()
        {
            if (_dataCache != null)
                return _dataCache;

            if (!File.Exists(_dataFile))
            {
                _dataCache = new DataObject();
                return _dataCache;
            }

            using var reader = new StreamReader(_dataFile, new FileStreamOptions
            {
                Access = FileAccess.Read,
                BufferSize = 4096,
                Mode = FileMode.Open,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan
            });

            var text = await reader.ReadToEndAsync();

            _dataCache = JsonConvert.DeserializeObject<DataObject>(text, _serializerSettings) ?? new DataObject();
            return _dataCache;
        }

        protected async Task SetDataAsync(DataObject data)
        {
            using var writer = new StreamWriter(_dataFile, new FileStreamOptions
            {
                Access = FileAccess.Write,
                BufferSize = 4096,
                Mode = FileMode.Create,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan
            });

            var text = JsonConvert.SerializeObject(data, _serializerSettings);
            await writer.WriteAsync(text);
            _dataCache = data;
        }

        public Task<bool> ContainsKey(StorageKey key) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                return data.Values.ContainsKey(StorageKeyConvert.Serialize(key));
            });

        public Task<Result<StorageResultReason>> Delete(StorageKey key) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                if (!data.Values.Remove(StorageKeyConvert.Serialize(key)))
                    return new(StorageResultReason.NotFound);
                await SetDataAsync(data);
                return new Result<StorageResultReason>();
            });

        protected T ExtractContainerValue<T>(StorageKey<T> key, DataLeaf leaf) where T : notnull
        {
            return ExtractContainerValue<T>(key.ToString(), leaf);
        }
        protected T ExtractContainerValue<T>(string key, DataLeaf leaf) where T : notnull
        {
            var container = leaf.ValueContainer.ToObject<ValueContainer<T>>()
                ?? throw new InvalidCastException($"Cannot convert {key} to type {typeof(T)}");
            return container.Value;
        }
        protected object ExtractContainerValue(StorageKey key, DataLeaf leaf)
        {
            var genericType = typeof(ValueContainer<>).MakeGenericType(key.Type);
            var container = leaf.ValueContainer.ToObject(genericType)
                ?? throw new InvalidCastException($"Cannot convert {key} to type {key.Type}");
            var genericParameter = genericType.GetProperty(nameof(ValueContainer<object>.Value))!;
            return genericParameter.GetValue(container)!;
        }

        public Task<Result<T, StorageResultReason>> Get<T>(StorageKey<T> key) where T : notnull =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                var stringKey = StorageKeyConvert.Serialize(key);
                if (!data.Values.TryGetValue(stringKey, out var value))
                    return new(StorageResultReason.NotFound);
                return new Result<T, StorageResultReason>(ExtractContainerValue<T>(key, value));
            });

        public Task Set<T>(StorageKey<T> key, T value) where T : notnull => SetMany<T>([(key, value)]);

        public Task<List<Result<object, StorageResultReason>>> GetMany(List<StorageKey> primaryKeys) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                return primaryKeys.Select(k => data.Values.TryGetValue(StorageKeyConvert.Serialize(k), out var leaf)
                ? new Result<object, StorageResultReason>(ExtractContainerValue(k, leaf))
                : new(StorageResultReason.NotFound)).ToList();
            });

        public Task<List<Result<T, StorageResultReason>>> GetMany<T>(List<StorageKey<T>> primaryKeys) where T : notnull =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                return primaryKeys.Select(k => data.Values.TryGetValue(StorageKeyConvert.Serialize(k), out var leaf)
                ? new Result<T, StorageResultReason>(ExtractContainerValue<T>(k, leaf))
                : new(StorageResultReason.NotFound)).ToList();
            });


        public Task<List<(StorageKey<T> Key, T Value)>> GetMany<T>(StorageKey<T> foreignKey) where T : notnull =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                var foreignKeyString = StorageKeyConvert.Serialize(foreignKey);
                return data.Values
                    .Where(kvp => kvp.Value.ForeignKeys.Contains(foreignKeyString))
                    .Select(kvp => (StorageKeyConvert.Deserialize<T>(kvp.Key),
                        ExtractContainerValue<T>(kvp.Key, kvp.Value)))
                    .ToList();
            });

        public Task Set<T>(StorageKey<T> primaryKey, T value, List<StorageKey<T>> addForeignKeys) where T : notnull =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                var primaryKeyString = StorageKeyConvert.Serialize(primaryKey);
                var foreignKeyStringSet = addForeignKeys
                    .Select(fk => StorageKeyConvert.Serialize(fk))
                    .ToHashSet();
                if (data.Values.TryGetValue(primaryKeyString, out var leaf))
                {
                    leaf.ValueContainer = JObject.FromObject(new ValueContainer<T> { Value = value }, _jObjectSerializer);
                    leaf.ForeignKeys.UnionWith(foreignKeyStringSet);
                    data.Values[primaryKeyString] = leaf;
                }
                else
                {
                    data.Values[primaryKeyString] = new DataLeaf
                    {
                        ValueContainer = JObject.FromObject(new ValueContainer<T> { Value = value }, _jObjectSerializer),
                        ForeignKeys = foreignKeyStringSet
                    };
                }

                await SetDataAsync(data);
            });

        public Task SetMany(List<(StorageKey Key, object Value)> values) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                foreach (var value in values)
                {
                    var primaryKeyString = StorageKeyConvert.Serialize(value.Key);
                    if (data.Values.TryGetValue(primaryKeyString, out var leaf))
                    {
                        leaf.ValueContainer = JObject.FromObject(new ValueContainer { Value = value.Value }, _jObjectSerializer);
                        data.Values[primaryKeyString] = leaf;
                        continue;
                    }

                    data.Values[primaryKeyString] = new DataLeaf
                    {
                        ValueContainer = JObject.FromObject(new ValueContainer { Value = value.Value }, _jObjectSerializer),
                    };
                }
                await SetDataAsync(data);
            });

        public Task SetMany<T>(List<(StorageKey<T> Key, T Value)> values) where T : notnull =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                foreach (var value in values)
                {
                    var primaryKeyString = StorageKeyConvert.Serialize(value.Key);
                    if (data.Values.TryGetValue(primaryKeyString, out var leaf))
                    {
                        leaf.ValueContainer = JObject.FromObject(new ValueContainer { Value = value.Value }, _jObjectSerializer);
                        data.Values[primaryKeyString] = leaf;
                        continue;
                    }

                    data.Values[primaryKeyString] = new DataLeaf
                    {
                        ValueContainer = JObject.FromObject(new ValueContainer { Value = value.Value }, _jObjectSerializer),
                    };
                }
                await SetDataAsync(data);
            });

        public Task<Result<int, StorageResultReason>> DeleteMany<T>(StorageKey<T> foreignKey) where T : notnull =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                var foreignKeyString = StorageKeyConvert.Serialize(foreignKey);
                var primaryKeysToDelete = data.Values
                    .Where(kvp => kvp.Value.ForeignKeys.Contains(foreignKeyString))
                    .Select(kvp => kvp.Key)
                    .ToList();

                var deleted = 0;
                foreach (var primaryKey in primaryKeysToDelete)
                    if (data.Values.Remove(primaryKey))
                        deleted++;

                if (deleted == 0)
                    return new(StorageResultReason.NotFound);
                await SetDataAsync(data);
                return new Result<int, StorageResultReason>(deleted);
            });
    }
}
