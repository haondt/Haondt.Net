using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Services;
using Haondt.Web.Services;
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
            services.AddSingleton<IIndexModelComponentFactory, IndexModelComponentFactory>();
            services.AddSingleton<IComponentDescriptor>(new ComponentDescriptor<IndexModel> { ViewPath = "~/Components/Index.cshtml" });
            services.AddSingleton<IComponentDescriptor>(new ComponentDescriptor<LoaderModel> { ViewPath = "~/Components/Loader.cshtml" });
            services.AddSingleton<IComponentDescriptor>(new ComponentDescriptor<AppendComponentLayoutModel> { ViewPath = "~/Components/AppendComponentLayout.cshtml" });
            services.AddSingleton<IEventPublisher, EventPublisher>();
            return services;
        }
    }
}
