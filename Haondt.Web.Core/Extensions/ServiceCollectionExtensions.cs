using Haondt.Web.Core.Components;
using Haondt.Web.Core.Services;

namespace Haondt.Web.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHaondtWebCoreServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IComponentFactory, ComponentFactory>();
            services.AddSingleton<IExceptionActionResultFactory, ExceptionActionResultFactory>();
            return services;
        }
    }
}
