﻿using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Haondt.Persistence.Services
{
    public class DataObject
    {
        public Dictionary<string, object?> Values { get; set; } = [];
    }

    public class FileStorage : IStorage
    {
        protected readonly string _dataFile;
        protected readonly JsonSerializerOptions _serializerSettings;
        protected DataObject? _dataCache;
        protected readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        public FileStorage(IOptions<HaondtFileStorageSettings> options)
        {
            _dataFile = options.Value.DataFile;
            _serializerSettings = new JsonSerializerOptions();
            ConfigureSerializerSettings(_serializerSettings);
        }

        protected virtual JsonSerializerOptions ConfigureSerializerSettings(JsonSerializerOptions settings)
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

            _dataCache = JsonSerializer.Deserialize<DataObject>(text, _serializerSettings) ?? new DataObject();
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

            var text = JsonSerializer.Serialize(data, _serializerSettings);
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

        public Task<Result<T, StorageResultReason>> Get<T>(StorageKey<T> key) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                var stringKey = StorageKeyConvert.Serialize(key);
                if (!data.Values.TryGetValue(stringKey, out var value))
                    return new(StorageResultReason.NotFound);
                if (value is not T castedValue)
                    throw new InvalidCastException($"Cannot convert {key} to type {typeof(T)}");
                return new Result<T, StorageResultReason>(castedValue);
            });

        public Task Set<T>(StorageKey<T> key, T value) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                data.Values[StorageKeyConvert.Serialize(key)] = value;
                await SetDataAsync(data);
            });
    }
}
