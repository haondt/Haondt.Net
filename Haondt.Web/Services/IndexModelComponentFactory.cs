using DotNext;
using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Microsoft.Extensions.Options;

namespace Haondt.Web.Services
{
    public class IndexModelComponentFactory(
        IOptions<IndexSettings> options,
        IComponentFactory componentFactory,
        ILayoutUpdateFactory layoutFactory,
        IEnumerable<IHeadEntryDescriptor> headEntries
        
        ) : IIndexModelComponentFactory
    {
        public async Task<Result<IComponent<IndexModel>>> GetComponent(string contentPage)
        {
            var loader = await componentFactory.GetPlainComponent(new LoaderModel { Target = $"/_component/{contentPage}" });
            if (!loader.IsSuccessful)
                return new(loader.Error);

            var layout = await layoutFactory.GetInitialLayout(loader.Value)
                .BuildAsync();

            if (!layout.IsSuccessful)
                return new(layout.Error);

            var index = await componentFactory.GetComponent(new IndexModel
            {
                Title = options.Value.SiteName,
                Content = layout.Value,
                HeadEntries = headEntries.Select(e => e.Render()).ToList()
            });

            return index;
        }
    }
}
