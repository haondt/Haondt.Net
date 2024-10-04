using Haondt.Core.Extensions;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.MongoDb.Converters;
using Haondt.Persistence.MongoDb.Discriminators;
using Haondt.Persistence.MongoDb.Serializers;
using Haondt.Persistence.MongoDb.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haondt.Persistence.MongoDb.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration, Action<MongoDbSettings>? configureSettings = null)
        {
            var settingsSection = configuration.GetSection(nameof(MongoDbSettings));
            if (settingsSection.Exists())
            {
                var settings = settingsSection.Get<MongoDbSettings>();
                if (settings != null)
                {
                    configureSettings?.Invoke(settings);
                    return AddMongoDb(services, settings);
                }
            }

            throw new ArgumentException($"Could not resolve {nameof(MongoDbSettings)} from configuration");
        }

        public static IServiceCollection AddMongoDb(this IServiceCollection services, MongoDbSettings settings)
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

            return services;
        }
    }
}
