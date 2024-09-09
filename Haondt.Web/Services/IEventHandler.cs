using Haondt.Core.Models;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;

namespace Haondt.Web.Services
{
    public interface IEventHandler
    {
        Task<Optional<IComponent>> HandleAsync(string eventName, IRequestData requestData);
    }
}
