using Haondt.Web.Core.Extensions;
using Haondt.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Haondt.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHaondtWebServices(this IServiceCollection services, IConfiguration configuration, HaondtWebOptions options)
        {
            services.AddHaondtWebCoreServices();

            services.AddSingleton<ILayoutComponentFactory, LayoutComponentFactory>();
            services.AddSingleton<IComponentFactory, ComponentFactory>();
            services.Configure<HtmxOptions>(o =>
            {
            });

            if (options.HtmxScriptUri.TryGetValue(out var htmxScriptUri))
                services.AddScoped<IHeadEntryDescriptor>(_ => new ScriptDescriptor
                {
                    Uri = htmxScriptUri
                });
            if (options.HyperscriptScriptUri.TryGetValue(out var hyperscriptScriptUri))
                services.AddScoped<IHeadEntryDescriptor>(_ => new ScriptDescriptor
                {
                    Uri = hyperscriptScriptUri
                });


            return services;
        }

        public static IServiceCollection AddHaondtWebServices(this IServiceCollection services, IConfiguration configuration, Action<HaondtWebOptions> configureOptions)
        {
            var options = new HaondtWebOptions();
            configureOptions(options);
            return services.AddHaondtWebServices(configuration, options);
        }
        public static IServiceCollection AddHaondtWebServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddHaondtWebServices(configuration, new HaondtWebOptions());
        }
    }
}
