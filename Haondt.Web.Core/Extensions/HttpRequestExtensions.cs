using Haondt.Web.Core.Http;

namespace Haondt.Web.Core.Extensions
{
    public static class HttpRequestExtensions
    {
        public static IRequestData AsRequestData(this HttpRequest request)
        {
            return new TransientRequestData(
                () => request.Form,
                () => request.Query,
                () => request.Cookies,
                () => request.Headers);
        }


    }
}
