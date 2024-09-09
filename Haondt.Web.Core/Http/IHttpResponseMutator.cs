using Haondt.Core.Models;

namespace Haondt.Web.Core.Http
{
    public interface IHttpResponseMutator
    {
        public Optional<Action<IHeaderDictionary>> ConfigureHeadersAction { get; set; }
        public Optional<int> SetStatusCode { get; set; }
    }
}
