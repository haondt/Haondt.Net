using Haondt.Web.Core.Components;

namespace Haondt.Web.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHaondtWebCoreServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<IComponentFactory, ComponentFactory>();
            return services;
        }
    }
}
