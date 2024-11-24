using Haondt.Web.BulmaCSS.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Services;

namespace Haondt.Web.BulmaCSS.Services
{
    public class BulmaCSSLayoutComponentFactory : ILayoutComponentFactory
    {
        public Task<IComponent> GetLayoutAsync(IComponent content)
        {
            return Task.FromResult<IComponent>(new Layout
            {
                NavigationBar = new(),
                Content = content
            });
        }
    }
}
