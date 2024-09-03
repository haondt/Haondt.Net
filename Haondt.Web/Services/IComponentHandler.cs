using Haondt.Web.Core.Components;

namespace Haondt.Web.Services
{
    public interface IComponentHandler
    {
        Task<IComponent> HandleAsync(string componentIdentity);
    }
}
