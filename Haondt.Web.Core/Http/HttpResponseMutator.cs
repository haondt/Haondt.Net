using DotNext;

namespace Haondt.Web.Core.Http
{
    public class HttpResponseMutator : IHttpResponseMutator
    {
        public Optional<Action<IHeaderDictionary>> ConfigureHeadersAction { get; set; }
        public Optional<int> SetStatusCode { get; set; }

        public IResponseData Apply(IResponseData responseData)
        {
            if (ConfigureHeadersAction)
                ConfigureHeadersAction.Value(responseData.Headers);
            if (SetStatusCode)
                responseData.StatusCode = SetStatusCode.Value;
            return responseData;
        }
    }
}
