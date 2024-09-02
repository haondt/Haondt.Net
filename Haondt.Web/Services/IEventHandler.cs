using DotNext;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;

namespace Haondt.Web.Services
{
    public interface IEventHandler
    {
        Task<Result<Optional<IComponent>>> HandleAsync(string eventName, IRequestData requestData);
    }
}
