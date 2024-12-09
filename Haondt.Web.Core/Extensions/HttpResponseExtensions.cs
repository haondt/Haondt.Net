using Haondt.Web.Core.Http;

namespace Haondt.Web.Core.Extensions
{
    public static class HttpResponseExtensions
    {
        public static IResponseData AsResponseData(this HttpResponse response)
        {
            return new TransientResponseData(
                () => response.Headers,
                c => response.StatusCode = c);
        }
    }
}
