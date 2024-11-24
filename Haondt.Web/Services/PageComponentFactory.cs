using Haondt.Web.Components;
using Haondt.Web.Core.Services;
using System.Web;

namespace Haondt.Web.Services
{

    public class PageComponentFactory(
        IComponentFactory componentFactory,
        ILayoutComponentFactory layoutFactory,
        IEnumerable<IHeadEntryDescriptor> headEntries) : IPageComponentFactory
    {

        public async Task<IResult> RenderPageAsync(string path, IReadOnlyDictionary<string, string>? query = null)
        {
            if (query != null && query.Count > 0)
            {
                var queryStrings = query.Select(tup => $"{HttpUtility.UrlEncode(tup.Key)}={HttpUtility.UrlEncode(tup.Value)}").ToList();
                path = $"{path}?{string.Join('&', queryStrings)}";
            }

            var loader = new Loader
            {
                Target = path,
            };

            var layout = await layoutFactory.GetLayoutAsync(loader);
            var page = new Page
            {
                Content = layout,
                HeadEntries = headEntries.Select(e => e.Render()).ToList()
            };

            return await componentFactory.RenderComponentAsync(page);
        }
    }
}
