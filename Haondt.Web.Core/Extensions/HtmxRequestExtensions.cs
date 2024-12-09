using Haondt.Core.Extensions;
using Haondt.Web.Core.Http;
namespace Haondt.Web.Core.Extensions
{
    public static class HtmxRequestExtensions
    {
        private const string HX_REQUEST = "Hx-Request";

        public static bool IsHxRequest(this IRequestData request)
        {
            return request.Headers.TryGetValue<bool>(HX_REQUEST).Or(false);
        }
    }
}
