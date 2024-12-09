using Haondt.Web.Core.Http;

namespace Haondt.Web.Core.Extensions
{
    public static class HtmxResponseExtensions
    {
        private const string HX_PUSH_URL = "HX-Push-Url";
        private const string HX_RESWAP = "HX-Reswap";
        private const string HX_RETARGET = "HX-Retarget";
        private const string HX_RESELECT = "HX-Reselect";

        /// <summary>
        /// Push a URL into the browser location history.
        /// This creates a new history entry, allowing navigation with the browser’s back and forward buttons.
        /// </summary>
        /// <remarks>
        /// <a href="https://htmx.org/headers/hx-push-url/"/>
        /// </remarks>
        /// <param name="responseData"></param>
        /// <param name="url">may be a relative or absolute url, or "false", which prevents the browser's history from being updated.</param>
        /// <returns></returns>
        public static IResponseData HxPushUrl(this IResponseData responseData, string url)
        {
            return responseData.Header(HX_PUSH_URL, url);
        }
        // TODO, idk how I want to solve this since it will be a json dict of events + payloads
        //public static IResponseData HxTriggerAfterSettle(this IResponseData responseData, string @event, object payload)
        //{
        //    return responseData.Header(HX_TRIGGER_AFTER_SETTLE, );
        //}

        /// <summary>
        /// Specify how the response will be swapped into the target element.
        /// </summary>
        /// <param name="method">default is "innerHTML"</param>
        /// <remarks>
        /// <a href="https://htmx.org/attributes/hx-swap/"/>
        /// </remarks>
        /// <returns></returns>
        public static IResponseData HxReswap(this IResponseData responseData, string method)
        {
            return responseData.Header(HX_RESWAP, method);
        }

        /// <summary>
        /// Specifies the target element to be swapped.
        /// </summary>
        /// <remarks>
        /// <a href="https://htmx.org/attributes/hx-target/"/>
        /// </remarks>
        /// <param name="responseData"></param>
        /// <param name="target">this or [closest|find|next|previous] &lt;css selector&gt;</param>
        /// <returns></returns>
        public static IResponseData HxRetarget(this IResponseData responseData, string target)
        {
            return responseData.Header(HX_RETARGET, target);
        }

        /// <summary>
        /// Specifies which part of the response is used to be swapped in.<br/>
        /// Overrides an existing hx-select on the triggering element
        /// </summary>
        /// <remarks>
        /// <a href="https://htmx.org/attributes/hx-select/"/>
        /// </remarks>
        /// <param name="responseData"></param>
        /// <param name="selector">css query selector to run on the response</param>
        /// <returns></returns>
        public static IResponseData HxReselect(this IResponseData responseData, string selector)
        {
            return responseData.Header(HX_RESELECT, selector);
        }
    }
}
