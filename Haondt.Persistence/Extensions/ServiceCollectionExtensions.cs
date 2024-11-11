using Haondt.Persistence.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Haondt.Persistence.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMemoryStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IStorage, MemoryStorage>();
            return services;
        }

        public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<HaondtFileStorageSettings>(configuration.GetSection(nameof(HaondtFileStorageSettings)));
            services.AddSingleton<FileStorage>();
            services.AddSingleton<IStorage>(sp => new TransientTransactionalBatchStorage(sp.GetRequiredService<FileStorage>()));
            return services;
        }
    }
}
