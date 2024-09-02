﻿using Haondt.Web.Core.Components;
using Haondt.Web.Demo.Components;

namespace Haondt.Web.Demo.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHaondtWebDemoServices(this IServiceCollection services)
        {
            services.AddSingleton<IComponentDescriptor>(new ComponentDescriptor<HomeModel>(new HomeModel())
            {
                ViewPath = "~/Components/Home.cshtml",
            });
            return services;
        }
    }
}