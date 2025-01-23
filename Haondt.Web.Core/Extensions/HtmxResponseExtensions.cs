using Haondt.Web.Core.Http;
using Newtonsoft.Json;

namespace Haondt.Web.Core.Extensions
{
    public static class HtmxResponseExtensions
    {
        private const string HX_PUSH_URL = "HX-Push-Url";
        private const string HX_RESWAP = "HX-Reswap";
        private const string HX_RETARGET = "HX-Retarget";
        private const string HX_RESELECT = "HX-Reselect";
        private const string HX_LOCATION = "HX-Location";
        private const string HX_TRIGGER = "HX-Trigger";

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
        /// Trigger client side actions on the target element within a response to htmx.
        /// </summary>
        /// <param name="responseData"></param>
        /// <param name="event">event to trigger</param>
        /// <param name="body">event details</param>
        /// <param name="target">target a different element with the event</param>
        /// <returns></returns>
        public static IResponseData HxTrigger(this IResponseData responseData,
            string @event,
            string? body = null,
            string? target = null)
        {
            return HxTrigger(responseData, @event, new Dictionary<string, string> { { "value", body ?? "" } }, target);
        }

        /// <summary>
        /// Trigger client side actions on the target element within a response to htmx.
        /// </summary>
        /// <param name="responseData"></param>
        /// <param name="event">event to trigger</param>
        /// <param name="body">event details</param>
        /// <param name="target">target a different element with the event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IResponseData HxTrigger(this IResponseData responseData,
            string @event,
            Dictionary<string, string> body,
            string? target = null)
        {
            foreach (var key in body.Keys)
                if ("target".Equals(key, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException($"Cannot use key {key}, \"target\" is a reserved keyword");

            return responseData.ReplaceHeader(HX_TRIGGER, existing =>
            {
                Dictionary<string, Dictionary<string, string>> existingPayload;
                if (existing.Length > 0 && existing[0] != null)
                {
                    try
                    {
                        existingPayload = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(existing[0]!)
                            ?? new();
                    }
                    catch
                    {
                        existingPayload = new();
                    }
                }
                else
                    existingPayload = new();

                existingPayload[@event] = body ?? new();
                if (target != null)
                    existingPayload[@event]["target"] = target;

                return JsonConvert.SerializeObject(existingPayload);
            });
        }

        /// <summary>
        /// Trigger a client side redirection without reloading the whole page.
        /// Instead of changing the page’s location it will act like following a hx-boost link,
        /// creating a new history entry, issuing an ajax request to the value of the header and
        /// pushing the path into history.
        /// </summary>
        /// <param name="responseData"></param>
        /// <param name="path">url to load the response from</param>
        /// <param name="source">the source element of the request</param>
        /// <param name="handler">a callback that wil handle the response HTML</param>
        /// <param name="values">values to submit with the request</param>
        /// <param name="headers">headers to submit with the request</param>
        /// <param name="target">the target to swap the response into</param>
        /// <param name="swap">how the repsonse will be swapped in relative to the target</param>
        /// <param name="select">allows you to select the content you want swapped from the response</param>
        /// <returns></returns>
        public static IResponseData HxLocation(this IResponseData responseData,
            string path,
            string? source = null,
            string? handler = null,
            string? target = null,
            string? swap = null,
            Dictionary<string, string>? values = null,
            Dictionary<string, string>? headers = null,
            string? select = null)
        {
            var payload = new Dictionary<string, object>
            {
                { "path", path }
            };
            if (source != null)
                payload["source"] = source;
            if (handler != null)
                payload["handler"] = handler;
            if (values != null)
                payload["values"] = values;
            if (headers != null)
                payload["headers"] = headers;
            if (target != null)
                payload["target"] = target;
            if (target != null)
                payload["target"] = target;
            if (swap != null)
                payload["swap"] = swap;
            if (select != null)
                payload["select"] = select;

            return responseData.Header(HX_LOCATION, JsonConvert.SerializeObject(payload));
        }


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
