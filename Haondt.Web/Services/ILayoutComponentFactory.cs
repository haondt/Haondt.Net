
using Haondt.Web.Core.Components;

namespace Haondt.Web.Services
{
    public interface ILayoutComponentFactory
    {
        public Task<IComponent> GetLayoutAsync(IComponent content, string targetComponentName);
    }

}
