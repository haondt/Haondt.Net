using Haondt.Core.Models;

namespace Haondt.Web.Extensions
{
    public class HaondtWebOptions
    {
        public Optional<string> HtmxScriptUri { get; set; } = "https://unpkg.com/htmx.org@2.0.4";
        public Optional<string> HyperscriptScriptUri { get; set; } = "https://unpkg.com/hyperscript.org@0.9.13";
    }
}
