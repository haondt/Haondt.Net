using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;
using System.Reflection;
using IComponent = Haondt.Web.Core.Components.IComponent;

namespace Haondt.Web.Core.Extensions
{
    public static class ComponentExtensions
    {
        private static ConcurrentDictionary<Type, PropertyInfo[]> ParameterCache = new();

        public static Dictionary<string, object?> ToDictionary<T>(this T component) where T : class, IComponent
        {
            return ToDictionary(component, typeof(T));
        }
        public static Dictionary<string, object?> ToDictionary(this IComponent component, Type componentType)
        {
            var parameters = ParameterCache.GetOrAdd(componentType,
                t => t.GetProperties()
                    .Where(p => p.GetCustomAttribute<ParameterAttribute>() != null)
                    .ToArray());

            return parameters
                .ToDictionary(p => p.Name, p => p.GetValue(component));
        }
    }
}
