﻿using DotNext;
using Haondt.Identity.StorageKey;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Haondt.Persistence.Services
{
    public class DataObject
    {
        public Dictionary<string, object?> Values = [];
    }

    public class FileStorage : IStorage
    {
        public string datafile = "./data.json";
        private readonly JsonSerializerOptions _serializerSettings;
        private DataObject? _dataCache;
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        public FileStorage()
        {
            _serializerSettings = new JsonSerializerOptions();
            ConfigureSerializerSettings(_serializerSettings);
        }

        private static JsonSerializerOptions ConfigureSerializerSettings(JsonSerializerOptions settings)
        {
            settings.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
            settings.WriteIndented = true;
            settings.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
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

            if (!File.Exists(datafile))
            {
                _dataCache = new DataObject();
                return _dataCache;
            }

            using var reader = new StreamReader(datafile, new FileStreamOptions
            {
                Access = FileAccess.Read,
                BufferSize = 4096,
                Mode = FileMode.Open,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan
            });

            var text = await reader.ReadToEndAsync();

            _dataCache = JsonSerializer.Deserialize<DataObject>(text, _serializerSettings) ?? new DataObject();
            return _dataCache;
        }

        protected async Task<Optional<Exception>> SetDataAsync(DataObject data)
        {
            using var writer = new StreamWriter(datafile, new FileStreamOptions
            {
                Access = FileAccess.Write,
                BufferSize = 4096,
                Mode = FileMode.Create,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan
            });

            var text = JsonSerializer.Serialize(data, _serializerSettings);
            await writer.WriteAsync(text);
            _dataCache = data;
            return new();
        }

        public Task<Result<bool>> ContainsKey(StorageKey key) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                return new Result<bool>(data.Values.ContainsKey(StorageKeyConvert.Serialize(key)));
            });

        public Task<Optional<Exception>> Delete(StorageKey key) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                if (!data.Values.Remove(StorageKeyConvert.Serialize(key)))
                    return new Optional<Exception>();
                await SetDataAsync(data);
                return new Optional<Exception>();
            });

        public Task<Result<T>> Get<T>(StorageKey<T> key) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                return new Result<T>((T)data.Values[StorageKeyConvert.Serialize(key)]!);
            });

        public Task<Optional<Exception>> Set<T>(StorageKey<T> key, T value) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                data.Values[StorageKeyConvert.Serialize(key)] = value;
                await SetDataAsync(data);
                return new Optional<Exception>();
            });

        public Task<Result<T>> TryGet<T>(StorageKey<T> key) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                var serializedKey = StorageKeyConvert.Serialize(key);
                if (data.Values.TryGetValue(serializedKey, out var value) && value is T castedValue)
                    return new Result<T>(castedValue);
                return new Result<T>(new KeyNotFoundException(serializedKey));
            });
    }
}