using Haondt.Web.Core.Http;

namespace Haondt.Web.Core.Extensions
{
    public static class HtmxResponseExtensions
    {
        private const string HX_PUSH_URL = "HX-Push-Url";
        private const string HX_RESWAP = "HX-Reswap";
        private const string HX_RETARGET = "HX-Retarget";
        private const string HX_RESELECT = "HX-Reselect";

        public static IResponseData HxPushUrl(this IResponseData responseData, string url)
        {
            return responseData.Header(HX_PUSH_URL, url);
        }
        // TODO, idk how I want to solve this since it will be a json dict of events + payloads
        //public static IResponseData HxTriggerAfterSettle(this IResponseData responseData, string @event, object payload)
        //{
        //    return responseData.Header(HX_TRIGGER_AFTER_SETTLE, );
        //}
        public static IResponseData HxReswap(this IResponseData responseData, string method)
        {
            return responseData.Header(HX_RESWAP, method);
        }
        public static IResponseData HxRetarget(this IResponseData responseData, string target)
        {
            return responseData.Header(HX_RETARGET, target);
        }
        public static IResponseData HxReselect(this IResponseData responseData, string selector)
        {
            return responseData.Header(HX_RESELECT, selector);
        }
    }
}
