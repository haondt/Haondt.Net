using Haondt.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Haondt.Web.Demo.UI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHaondtWebDemoUI(this IServiceCollection services)
        {
            services.AddScoped<IHeadEntryDescriptor>(_ => new StyleSheetDescriptor
            {
                Uri = "/static/Haondt.Web.Demo.styles.css",
            });

            services.AddScoped<IHeadEntryDescriptor>(_ => new StyleSheetDescriptor
            {
                Uri = "/static/shared/styles.css"
            });

            return services;
        }
    }
}
