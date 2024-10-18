using Haondt.Identity.StorageKey;
using Haondt.Persistence.MongoDb.Converters;
using Haondt.Persistence.MongoDb.Discriminators;
using Haondt.Persistence.MongoDb.Serializers;
using Haondt.Persistence.MongoDb.Services;
using Haondt.Persistence.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

namespace Haondt.Persistence.MongoDb.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDbStorage(this IServiceCollection services, IConfiguration configuration, Action<MongoDbSettings>? configureSettings = null)
        {
            var settingsSection = configuration.GetSection(nameof(MongoDbSettings));
            if (settingsSection.Exists())
            {
                var settings = settingsSection.Get<MongoDbSettings>();
                if (settings != null)
                {
                    configureSettings?.Invoke(settings);
                    return AddMongoDbStorage(services, settings);
                }
            }

            throw new ArgumentException($"Could not resolve {nameof(MongoDbSettings)} from configuration");
        }

        public static IServiceCollection AddMongoDbStorage(this IServiceCollection services, MongoDbSettings settings)
        {
            services.AddSingleton<IMongoClient>(sp =>
            {
                var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
                if (settings.LogCommands)
                {
                    var logger = sp.GetRequiredService<ILogger<MongoDbSettings>>();
                    clientSettings.ClusterConfigurator = cb =>
                    {
                        cb.Subscribe<CommandStartedEvent>(e =>
                        {
                            logger.LogInformation("Excecuting mongodb command {commandName} with body {commandJson}", e.CommandName, e.Command.ToJson());
                        });
                    };
                }
                return new MongoClient(clientSettings);
            });


            if (settings.RegisterIndiscriminateObjectSerializer)
            {
                if (settings.RegisterSimpleTypeDiscriminatorConvention)
                    BsonSerializer.RegisterSerializer(new ObjectSerializer(new SimpleTypeDiscriminatorConvention(), type => true));
                else
                    BsonSerializer.RegisterSerializer(new ObjectSerializer(type => true));
            }
            if (settings.RegisterNewtonsoftJsonSerializers)
            {
                BsonSerializer.RegisterSerializer(new JObjectSerializer());
                BsonSerializer.RegisterSerializer(new JArraySerializer());
                BsonSerializer.RegisterSerializer(new JValueSerializer());
                BsonSerializer.RegisterSerializer(new JTokenSerializer());
            }
            if (settings.RegisterSimpleTypeDiscriminatorConvention)
                ConventionRegistry.Register(nameof(Haondt), new ConventionPack
                {
                    new SimpleTypeDiscriminatorConvention()
                }, type => true);

            BsonSerializer.RegisterGenericSerializerDefinition(typeof(StorageKey<>), typeof(StorageKeyBsonConverter<>));
            BsonSerializer.RegisterSerializer(typeof(StorageKey), new StorageKeyBsonConverter());

            services.AddSingleton<IStorage>(sp =>
                new MongoDbStorage(settings.Database, settings.Collection, sp.GetRequiredService<IMongoClient>()));

            return services;
        }
    }
}
