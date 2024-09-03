using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haondt.Core.Extensions
{
    public static class ConfigurationExtensions
    {
        public static T GetSection<T>(this IConfiguration configuration) where T : class, new()
        {
            var section = new T();
            var configurationSection = configuration.GetSection(typeof(T).Name);
            if (configurationSection.Exists())
                configurationSection.Bind(section);
            return section;
        }
    }
}

