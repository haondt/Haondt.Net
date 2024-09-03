using Haondt.Web.Assets;
using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Services;
using Haondt.Web.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haondt.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHaondtWebServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<IndexSettings>(configuration.GetSection(nameof(IndexSettings)));
            services.AddScoped<IIndexModelComponentFactory, IndexModelComponentFactory>();
            services.AddScoped<IComponentDescriptor>(_ => new ComponentDescriptor<IndexModel> { ViewPath = "~/Components/Index.cshtml" });
            services.AddScoped<IComponentDescriptor>(_ => new ComponentDescriptor<LoaderModel> { ViewPath = "~/Components/Loader.cshtml" });
            services.AddScoped<IComponentDescriptor>(_ => new ComponentDescriptor<AppendComponentLayoutModel> { ViewPath = "~/Components/AppendComponentLayout.cshtml" });
            services.AddScoped<IEventPublisher, EventPublisher>();
            services.AddSingleton<IAssetProvider, AssetProvider>();
            services.AddSingleton<FileExtensionContentTypeProvider>();
            services.AddTransient<IMemoryCache, MemoryCache>();
            services.AddScoped<IAssetHandler, AssetHandler>();
            services.AddScoped<IComponentHandler, ComponentHandler>();
            return services;
        }
    }
}
