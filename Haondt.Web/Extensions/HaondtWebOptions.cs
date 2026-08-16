using Haondt.Core.Models;

namespace Haondt.Web.Extensions
{
    public class HaondtWebOptions
    {
        public Optional<string> HtmxScriptUri { get; set; } = "/static/haondt/Haondt.Web/vendored/htmx.org/dist/htmx.min.js";
        public Optional<string> HyperscriptScriptUri { get; set; } = "/static/haondt/Haondt.Web/vendored/hyperscript.org/dist/_hyperscript.min.js";
    }
}
