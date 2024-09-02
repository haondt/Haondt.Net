using DotNext;

namespace Haondt.Web.Core.Http
{
    public class HttpResponseMutator : IHttpResponseMutator
    {
        public Optional<Action<IHeaderDictionary>> ConfigureHeadersAction { get; set; }

        public IResponseData Apply(IResponseData responseData)
        {
            if (ConfigureHeadersAction)
                ConfigureHeadersAction.Value(responseData.Headers);
            return responseData;
        }
    }
}
