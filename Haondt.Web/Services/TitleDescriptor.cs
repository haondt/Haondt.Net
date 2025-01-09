using Haondt.Core.Extensions;
using Haondt.Core.Models;

namespace Haondt.Web.Services
{
    public class TitleDescriptor : IHeadEntryDescriptor
    {
        public required string Title { get; init; }
        public Optional<string> Id { get; init; }

        public string Render() => $"<title{Id.As(s => $" id=\"{s}\"").Or("")}>{Title}</title>";
    }
}
