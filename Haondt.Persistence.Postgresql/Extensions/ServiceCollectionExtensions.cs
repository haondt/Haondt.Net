using Haondt.Persistence.Postgresql.Services;
using Haondt.Persistence.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Haondt.Persistence.Postgresql.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPostgresqlStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PostgresqlStorageSettings>(configuration.GetSection(nameof(PostgresqlStorageSettings)));
            services.AddSingleton<IStorage, PostgresqlStorage>();
            return services;
        }
    }
}
