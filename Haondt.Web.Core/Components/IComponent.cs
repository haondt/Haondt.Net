using Haondt.Web.Core.Http;

namespace Haondt.Web.Core.Components
{
    public interface IComponent
    {
        public IResponseData Response { get; }
        public IRequestData Request { get; }
    }
}
