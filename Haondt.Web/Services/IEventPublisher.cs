using DotNext;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;

namespace Haondt.Web.Services
{
    public interface IEventPublisher
    {
        Task<Result<Optional<IComponent>>> PublishAsync(string eventName, IRequestData requestData);
    }
}
