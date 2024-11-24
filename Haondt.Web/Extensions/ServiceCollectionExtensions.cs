using Haondt.Web.Assets;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;

namespace Haondt.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHaondtWebServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHaondtWebCoreServices();

            services.AddScoped<IPageComponentFactory, PageComponentFactory>();
            services.AddSingleton<IAssetProvider, AssetProvider>();
            services.AddSingleton<FileExtensionContentTypeProvider>();
            services.AddTransient<IMemoryCache, MemoryCache>();
            services.AddScoped<IAssetHandler, AssetHandler>();

            return services;
        }
    }
}
