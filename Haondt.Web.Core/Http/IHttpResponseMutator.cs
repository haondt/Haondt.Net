using DotNext;

namespace Haondt.Web.Core.Http
{
    public interface IHttpResponseMutator
    {
        public Optional<Action<IHeaderDictionary>> ConfigureHeadersAction { get; set; }
    }
}
