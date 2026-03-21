using Haondt.Core.Models;

namespace Haondt.Web.Services
{
    public class LinkDescriptor : IHeadEntryDescriptor
    {
        public Optional<string> Relationship { get; set; }
        public Optional<string> Type { get; set; }
        public required string Uri { get; set; }
        public Union<string, bool>? CrossOrigin { get; set; }
        public Optional<string> As { get; set; }

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

            if (As.TryGetValue(out var @as))
                parts.Add($"as=\"{@as}\"");

            if (CrossOrigin is { } crossOrigin)
            {
                var crossOriginValue = crossOrigin.Is<string>(out var v) ? $"=\"{v}\"" : "";
                parts.Add($"crossorigin{crossOriginValue}");
            }


            return $"<link {string.Join(' ', parts)} />";
        }
    }
}
