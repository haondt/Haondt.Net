using Haondt.Core.Models;

namespace Haondt.Web.Services
{
    public class LinkDescriptor : IHeadEntryDescriptor
    {
        public Optional<string> Relationship { get; set; }
        public Optional<string> Type { get; set; }
        public required string Uri { get; set; }

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

            return $"<link {string.Join(' ', parts)} />";
        }
    }
}
