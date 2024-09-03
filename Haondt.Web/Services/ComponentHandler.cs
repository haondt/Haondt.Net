using Haondt.Web.Core.Components;

namespace Haondt.Web.Services
{
    public class ComponentHandler(IComponentFactory componentFactory) : IComponentHandler
    {
        public async Task<IComponent> HandleAsync(string componentIdentity)
        {
            var component = await componentFactory.GetComponent(componentIdentity);
            return component.Value;
        }
    }
}
