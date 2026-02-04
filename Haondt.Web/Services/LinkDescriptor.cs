using Haondt.Core.Extensions;
using Haondt.Core.Models;

namespace Haondt.Web.Services
{
    public class LinkDescriptor : IHeadEntryDescriptor
    {
        public Optional<string> Relationship { get; set; }
        public Optional<string> Type { get; set; }
        public required string Uri { get; set; }
        public Optional<Optional<string>> CrossOrigin { get; set; }

        public string Render()
        {
            var parts = new List<string>
            {
                $"href=\"{Uri}\""
            };

            if (Relationship.TryGetValue(out var relationship))
                parts.Add($"rel=\"{relationship}\"");

            if (Type.TryGetValue(out var type))
                parts.Add($"type=\"{type}\"");

            if (CrossOrigin.TryGetValue(out var crossOrigin))
            {
                var crossOriginValue = crossOrigin.Map(v => $"=\"{v}\"").Or("");
                parts.Add($"crossorigin{crossOriginValue}");
            }


            return $"<link {string.Join(' ', parts)} />";
        }
    }
}
