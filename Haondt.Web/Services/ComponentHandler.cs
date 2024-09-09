using Haondt.Web.Core.Components;

namespace Haondt.Web.Services
{
    public class ComponentHandler(IComponentFactory componentFactory) : IComponentHandler
    {
        public Task<IComponent> HandleAsync(string componentIdentity)
        {
            return componentFactory.GetComponent(componentIdentity);
        }
    }
}
