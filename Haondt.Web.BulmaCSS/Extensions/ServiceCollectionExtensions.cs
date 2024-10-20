﻿using Haondt.Web.Assets;
using Haondt.Web.BulmaCSS.Components;
using Haondt.Web.BulmaCSS.Services;
using Haondt.Web.Components.Services;
using Haondt.Web.Core.Components;
using Haondt.Web.Services;
using Microsoft.Extensions.Options;

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
            services.AddScoped<ILayoutComponentFactory, BulmaCSSDefaultLayoutComponentFactory>();
            services.AddScoped<IEventHandler, BulmaCSSEventHandler>();
            return services;
        }

        public static IServiceCollection AddBulmaCSSComponents(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<NavigationBarSettings>(configuration.GetSection(nameof(NavigationBarSettings)));
            services.AddScoped<IComponentDescriptor>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<NavigationBarSettings>>();
                var indexOptions = sp.GetRequiredService<IOptions<IndexSettings>>();
                return new ComponentDescriptor<NavigationBarModel>(new NavigationBarModel
                {
                    LogoClickUri = new(indexOptions.Value.HomePage),
                    LogoUri = string.IsNullOrEmpty(options.Value.LogoUri) ? new() : new(options.Value.LogoUri),
                    NavigationBarEntries = options.Value.Entries.Select(e => new Components.NavigationBarEntry
                    {
                        Title = e.Title,
                        PushUrl = string.IsNullOrEmpty(e.PushUrl) ? new() : new(e.PushUrl),
                        Url = e.Url,
                    }).ToList()
                })
                {
                    ViewPath = "~/Components/NavigationBar.cshtml"
                };
            });

            services.AddScoped<IComponentDescriptor>(_ => new ComponentDescriptor<DefaultLayoutModel>
            {
                ViewPath = "~/Components/DefaultLayout.cshtml"
            });
            services.AddScoped<IComponentDescriptor>(_ => new ComponentDescriptor<ToastModel>
            {
                ViewPath = "~/Components/Toast.cshtml"
            });

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
