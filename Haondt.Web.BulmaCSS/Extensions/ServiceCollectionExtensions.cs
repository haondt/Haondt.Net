using Haondt.Web.BulmaCSS.Services;
using Haondt.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Haondt.Web.BulmaCSS.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseBulmaCSS(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddBulmaCSSHeadEntries();
            services.AddBulmaCSSServices(configuration);

            return services;
        }

        public static IServiceCollection AddBulmaCSSServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ILayoutComponentFactory, BulmaCSSLayoutComponentFactory>();
            return services;
        }


        public static IServiceCollection AddBulmaCSSHeadEntries(this IServiceCollection services)
        {
            services.AddScoped<IHeadEntryDescriptor>(_ => new StyleSheetDescriptor
            {
                Uri = "https://cdn.jsdelivr.net/npm/bulma@1.0.2/css/bulma.min.css"
            });

            services.AddScoped<IHeadEntryDescriptor>(_ => new StyleSheetDescriptor
            {
                Uri = "/static/haondt/Haondt.Web.BulmaCSS/styles.css"
            });

            return services;
        }
    }
}
