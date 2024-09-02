using Haondt.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haondt.Web.BulmaCSS.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseBulmaCSS(this IServiceCollection services)
        {
            services.AddBulmaCSSHeadEntries();
            return services;
        }

        public static IServiceCollection AddBulmaCSSHeadEntries(this IServiceCollection services)
        {
            services.AddSingleton<IHeadEntryDescriptor>(new StyleSheetDescriptor
            {
                Uri = "https://cdn.jsdelivr.net/npm/bulma@1.0.2/css/bulma.min.css"
            });

            return services;

        }
    }
}
