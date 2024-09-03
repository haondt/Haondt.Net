using Haondt.Core.Extensions;
using Haondt.Persistence.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Haondt.Persistence.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHaondtPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            var persistenceSettings = configuration.GetSection<PersistenceSettings>();
            switch (persistenceSettings.Driver)
            {
                case PersistenceDrivers.Memory:
                    services.AddSingleton<IStorage, MemoryStorage>();
                    break;
                case PersistenceDrivers.File:
                    services.AddSingleton<IStorage, FileStorage>();
                    break;
            }

            return services;
        }
    }
}
