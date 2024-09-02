using DotNext;
using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Microsoft.Extensions.Options;

namespace Haondt.Web.Services
{
    public class IndexModelComponentFactory(
        IOptions<IndexSettings> options,
        IComponentFactory componentFactory,
        IEnumerable<IHeadEntryDescriptor> headEntries
        
        ) : IIndexModelComponentFactory
    {
        public async Task<Result<IComponent<IndexModel>>> GetComponent(string contentPage)
        {
            var loader = await componentFactory.GetPlainComponent(new LoaderModel { Target = $"/{contentPage}" });
            if (!loader.IsSuccessful)
                return new(loader.Error);

            var index = await componentFactory.GetComponent(new IndexModel
            {
                Title = options.Value.SiteName,
                Content = loader.Value,
                HeadEntries = headEntries.Select(e => e.Render()).ToList()
            });

            return index;
        }
    }
}
