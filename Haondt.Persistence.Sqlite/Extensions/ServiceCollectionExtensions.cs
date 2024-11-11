using Haondt.Persistence.Services;
using Haondt.Persistence.Sqlite.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Haondt.Persistence.Sqlite.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqliteStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SqliteStorageSettings>(configuration.GetSection(nameof(SqliteStorageSettings)));
            services.AddSingleton<SqliteStorage>();
            services.AddSingleton<IStorage>(sp => new TransientTransactionalBatchStorage(sp.GetRequiredService<SqliteStorage>()));
            return services;
        }
    }
}
