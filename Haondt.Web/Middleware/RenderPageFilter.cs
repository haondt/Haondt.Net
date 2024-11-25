using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using Haondt.Web.Core.Services;
using Haondt.Web.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace Haondt.Web.Middleware
{
    public class RenderPageFilter(
        IComponentFactory componentFactory,
        ILayoutComponentFactory layoutFactory,
        IEnumerable<IHeadEntryDescriptor> headEntries) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context.HttpContext.Request.AsRequestData();
            if (request.IsHxRequest())
            {
                await next();
                return;
            }

            var loader = GenerateLoader(request);
            var layout = await layoutFactory.GetLayoutAsync(loader);
            var page = new Page
            {
                Content = layout,
                HeadEntries = headEntries.Select(e => e.Render()).ToList()
            };

            IResult component = await componentFactory.RenderComponentAsync(page);
            await component.ExecuteAsync(context.HttpContext);
        }

        private Loader GenerateLoader(IRequestData request)
        {
            var method = $"hx-{request.Method.ToLower()}";
            var loader = new Loader
            {
                HxMethod = method,
                Target = request.Path,
            };
            List<(string Key, string Value)> vals = new();

            static void addValues(IEnumerable<KeyValuePair<string, StringValues>> values, List<(string Key, string Value)> vals)
            {
                foreach (var (key, stringValues) in values)
                {
                    if (string.IsNullOrEmpty(key))
                        continue;
                    foreach (var stringValue in stringValues)
                        vals.Add((key, stringValue ?? ""));
                }
            }

            try
            {
                addValues(request.Query, vals);
            }
            catch { }
            try
            {
                addValues(request.Form, vals);
            }
            catch { }

            if (vals.Count > 0)
                loader.Values = new(vals);

            return loader;
        }

    }
}
