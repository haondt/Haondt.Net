using Haondt.Web.BulmaCSS.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Services;

namespace Haondt.Web.BulmaCSS.Services
{
    public class BulmaCSSDefaultLayoutComponentFactory(IComponentFactory componentFactory) : ILayoutComponentFactory
    {
        public async Task<IComponent> GetLayoutAsync(IComponent content, string targetComponentIdentity)
        {
            var navigationBar = await componentFactory.GetPlainComponent<NavigationBarModel>();
            return await componentFactory.GetPlainComponent(new DefaultLayoutModel
            {
                Content = content,
                NavigationBar = navigationBar
            });
        }
    }
}
