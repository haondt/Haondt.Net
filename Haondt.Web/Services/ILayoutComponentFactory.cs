using Microsoft.AspNetCore.Components;

namespace Haondt.Web.Services
{
    public interface ILayoutComponentFactory
    {
        public Task<IComponent> GetLayoutAsync(IComponent content);
    }

}
