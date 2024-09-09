using Haondt.Core.Models;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;

namespace Haondt.Web.Services
{
    public interface IEventPublisher
    {
        Task<Optional<IComponent>> PublishAsync(string eventName, IRequestData requestData);
    }
}
