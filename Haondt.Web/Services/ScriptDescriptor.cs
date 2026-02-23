using Haondt.Core.Models;
using System.Text;

namespace Haondt.Web.Services
{
    public class ScriptDescriptor : IHeadEntryDescriptor
    {
        public required string Uri { get; init; }
        public Optional<string> CrossOrigin { get; init; }
        public Optional<string> Type { get; init; }
        public string Render()
        {
            var attributes = new Dictionary<string, string>();
            attributes["src"] = Uri;
            if (CrossOrigin.TryGetValue(out var crossOrigin))
                attributes["crossorigin"] = crossOrigin;
            if (Type.TryGetValue(out var type))
                attributes["type"] = type;

            return attributes.Aggregate(new StringBuilder("<script "),
                (sb, kvp) => sb.Append($"{kvp.Key}=\"{kvp.Value}\""))
                .Append("></script>")
                .ToString();
        }
    }
}
