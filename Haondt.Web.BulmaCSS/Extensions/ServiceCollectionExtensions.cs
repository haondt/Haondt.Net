using Haondt.Web.Assets;
using Haondt.Web.BulmaCSS.Services;
using Haondt.Web.Services;

namespace Haondt.Web.BulmaCSS.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseBulmaCSS(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddBulmaCSSHeadEntries();
            services.AddBulmaCSSServices(configuration);
            services.AddBulmaCSSComponents(configuration);
            services.AddBulmaCSSAssetSources();

            return services;
        }

        public static IServiceCollection AddBulmaCSSServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ILayoutComponentFactory, BulmaCSSLayoutComponentFactory>();
            return services;
        }

        public static IServiceCollection AddBulmaCSSComponents(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }

        public static IServiceCollection AddBulmaCSSAssetSources(this IServiceCollection services)
        {
            var assembly = typeof(ServiceCollectionExtensions).Assembly;
            services.AddSingleton<IAssetSource>(sp => new ManifestAssetSource(assembly));
            return services;
        }

        public static IServiceCollection AddBulmaCSSHeadEntries(this IServiceCollection services)
        {
            services.AddScoped<IHeadEntryDescriptor>(_ => new StyleSheetDescriptor
            {
                Uri = "https://cdn.jsdelivr.net/npm/bulma@1.0.2/css/bulma.min.css"
            });

            var assemblyPrefix = typeof(ServiceCollectionExtensions).Assembly.GetName().Name;
            services.AddScoped<IHeadEntryDescriptor>(sp => new StyleSheetDescriptor
            {
                Uri = $"/_asset/{assemblyPrefix}.wwwroot.styles.css"
            });

            return services;

        }
    }
}
