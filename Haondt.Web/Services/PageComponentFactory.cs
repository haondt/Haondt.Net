using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using System.Web;

namespace Haondt.Web.Services
{

    public class PageComponentFactory(
        IComponentFactory componentFactory,
        ILayoutComponentFactory layoutFactory,
        IEnumerable<IHeadEntryDescriptor> headEntries) : IPageComponentFactory
    {
        public async Task<IComponent<PageModel>> GetComponent(string targetComponentName, List<(string Key, string Value)> queryParams)
        {
            var componentUri = $"/_component/{targetComponentName}";
            var queryStrings = queryParams.Select(tup => $"{HttpUtility.UrlEncode(tup.Key)}={HttpUtility.UrlEncode(tup.Value)}").ToList();
            if (queryStrings.Count > 0)
                componentUri = $"{componentUri}?{string.Join('&', queryStrings)}";

            var loader = await componentFactory.GetPlainComponent(new LoaderModel
            {
                Target = componentUri
            });
            var layout = await layoutFactory.GetLayoutAsync(loader, targetComponentName);
            var pageComponent = await componentFactory.GetComponent(new PageModel
            {
                Content = layout,
                HeadEntries = headEntries.Select(e => e.Render()).ToList()
            });

            return pageComponent;
        }

        public Task<IComponent<PageModel>> GetComponent<T>() where T : IComponentModel
        {
            return GetComponent(ComponentDescriptor<T>.TypeIdentity);
        }

        public Task<IComponent<PageModel>> GetComponent(string targetComponentName)
        {
            return GetComponent(targetComponentName, new List<(string Key, string Value)>());
        }

        public Task<IComponent<PageModel>> GetComponent(string targetComponentName, Dictionary<string, string> queryParams)
        {
            return GetComponent(targetComponentName, queryParams.Select(kvp => (kvp.Key, kvp.Value)).ToList());
        }

        public Task<IComponent<PageModel>> GetComponent<T>(Dictionary<string, string> queryParams) where T : IComponentModel
        {
            return GetComponent<T>(queryParams.Select(kvp => (kvp.Key, kvp.Value)).ToList());
        }

        public Task<IComponent<PageModel>> GetComponent<T>(List<(string Key, string Value)> queryParams) where T : IComponentModel
        {
            return GetComponent(ComponentDescriptor<T>.TypeIdentity, queryParams);
        }
    }
}
